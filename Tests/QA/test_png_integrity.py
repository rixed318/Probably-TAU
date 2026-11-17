"""Standalone PNG signature/IHDR/dimensions/SHA-256 QA gate (Issue #21).

Run: python3 Tests/QA/test_png_integrity.py -v
Validate actual file: python3 Tests/QA/test_png_integrity.py --file PATH --width 320 --height 180 --sha256 HEX
Checks only header integrity and identity, not full PNG decoding, originality or rights.
"""
import argparse
import hashlib
from pathlib import Path
import struct
import sys
import unittest
import zlib

SIGNATURE = b'\x89PNG\r\n\x1a\n'


def verify_png(path, width, height, expected_sha256):
    data = Path(path).read_bytes()
    if not data.startswith(SIGNATURE):
        raise ValueError('invalid PNG signature')
    if len(data) < 33 or data[12:16] != b'IHDR' or struct.unpack('>I', data[8:12])[0] != 13:
        raise ValueError('missing/invalid IHDR')
    ihdr = data[12:29]
    crc = struct.unpack('>I', data[29:33])[0]
    if zlib.crc32(ihdr) & 0xffffffff != crc:
        raise ValueError('IHDR CRC mismatch')
    actual_width, actual_height = struct.unpack('>II', data[16:24])
    if (actual_width, actual_height) != (width, height):
        raise ValueError(f'dimension mismatch: {actual_width}x{actual_height}')
    if not actual_width or not actual_height:
        raise ValueError('zero PNG dimension')
    bit_depth, color_type, compression, filter_method, interlace = data[24:29]
    allowed_depths = {0: {1, 2, 4, 8, 16}, 2: {8, 16}, 3: {1, 2, 4, 8}, 4: {8, 16}, 6: {8, 16}}
    if (bit_depth not in allowed_depths.get(color_type, set()) or compression != 0
            or filter_method != 0 or interlace not in (0, 1)):
        raise ValueError('invalid IHDR encoding')
    actual_sha = hashlib.sha256(data).hexdigest()
    if actual_sha.lower() != expected_sha256.lower():
        raise ValueError('SHA-256 mismatch')
    return actual_sha


def sample_header(width=320, height=180, bit_depth=8, color_type=6,
                  compression=0, filter_method=0, interlace=0):
    payload = struct.pack('>IIBBBBB', width, height, bit_depth, color_type,
                          compression, filter_method, interlace)
    chunk = b'IHDR' + payload
    return SIGNATURE + struct.pack('>I', 13) + chunk + struct.pack('>I', zlib.crc32(chunk) & 0xffffffff)


class TestPngIntegrity(unittest.TestCase):
    def setUp(self):
        import tempfile
        self.tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self.tmp.cleanup)
        self.path = Path(self.tmp.name) / 'candidate.png'
        self.data = sample_header()
        self.path.write_bytes(self.data)
        self.sha = hashlib.sha256(self.data).hexdigest()

    def test_valid_header_dimensions_hash(self):
        self.assertEqual(verify_png(self.path, 320, 180, self.sha), self.sha)

    def test_invalid_signature(self):
        self.path.write_bytes(b'BAD!' + self.data[4:])
        with self.assertRaisesRegex(ValueError, 'signature'):
            verify_png(self.path, 320, 180, self.sha)

    def test_wrong_dimensions(self):
        with self.assertRaisesRegex(ValueError, 'dimension'):
            verify_png(self.path, 320, 181, self.sha)

    def test_wrong_hash(self):
        with self.assertRaisesRegex(ValueError, 'SHA-256'):
            verify_png(self.path, 320, 180, '0' * 64)

    def test_corrupt_ihdr_crc(self):
        self.path.write_bytes(self.data[:29] + b'\x00\x00\x00\x00')
        with self.assertRaisesRegex(ValueError, 'CRC'):
            verify_png(self.path, 320, 180, self.sha)

    def check_invalid_encoding(self, **kwargs):
        data = sample_header(**kwargs)
        self.path.write_bytes(data)
        with self.assertRaisesRegex(ValueError, 'IHDR encoding'):
            verify_png(self.path, 320, 180, hashlib.sha256(data).hexdigest())

    def test_invalid_color_type(self):
        self.check_invalid_encoding(color_type=5)

    def test_invalid_bit_depth_for_rgba(self):
        self.check_invalid_encoding(bit_depth=4)

    def test_invalid_compression_method(self):
        self.check_invalid_encoding(compression=1)

    def test_invalid_filter_method(self):
        self.check_invalid_encoding(filter_method=1)

    def test_invalid_interlace_method(self):
        self.check_invalid_encoding(interlace=2)


if __name__ == '__main__':
    if '--file' in sys.argv:
        parser = argparse.ArgumentParser()
        parser.add_argument('--file', required=True)
        parser.add_argument('--width', type=int, required=True)
        parser.add_argument('--height', type=int, required=True)
        parser.add_argument('--sha256', required=True)
        args = parser.parse_args()
        print(verify_png(args.file, args.width, args.height, args.sha256))
    else:
        unittest.main()
