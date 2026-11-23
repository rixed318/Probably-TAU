#!/usr/bin/env python3
"""Preflight an unapproved ART-A0.1 office PNG; does not grant art approval."""
import argparse
import hashlib
import struct
import sys
from pathlib import Path

PALETTE = set(bytes.fromhex(c) for c in (
    '101C20','18272B','132C28','18392F','2E5545','47705A',
    '72907A','567A80','8BAAAC','C5D4C8','503828','76513B',
    'A17750','8B6B43','C9A36C','E3C78C','CABF9E','E8DEC3',
    'F7EDDA','865B4A','C28B69','E1B594','A45E55','63727C'))

def verify(path):
    raw = Path(path).read_bytes()
    if len(raw) < 33 or raw[:8] != b'\x89PNG\r\n\x1a\n':
        raise ValueError('invalid PNG signature')
    if raw[12:16] != b'IHDR' or struct.unpack('>II',raw[16:24]) != (320,180):
        raise ValueError('expected 320x180 PNG IHDR')
    from PIL import Image
    from io import BytesIO
    with Image.open(BytesIO(raw)) as im:
        im.load()
        if im.format != 'PNG' or im.mode != 'RGBA':
            raise ValueError('expected PNG RGBA8')
        colors = set(im.getdata())
    if any(a != 255 for *_,a in colors):
        raise ValueError('office background must be fully opaque')
    if any(bytes(rgb) not in PALETTE for *rgb,_ in colors):
        raise ValueError('pixels outside ART-A0.1 master palette')
    if len(colors) > 24:
        raise ValueError('more than 24 colors')
    return len(colors), hashlib.sha256(raw).hexdigest()

if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('png', type=Path)
    p.add_argument('--expected-sha256', help='Optional SHA-256 provenance checksum')
    args = p.parse_args()
    try:
        if args.expected_sha256 is not None:
            expected = args.expected_sha256.lower()
            if len(expected) != 64 or any(c not in '0123456789abcdef' for c in expected):
                raise ValueError('expected SHA-256 must be 64 hexadecimal characters')
        count, digest = verify(args.png)
        if args.expected_sha256 is not None and digest != expected:
            raise ValueError(f'SHA-256 mismatch: got {digest}, expected {expected}')
    except (OSError, ValueError, ImportError) as e:
        print(f'FAIL: {e}', file=sys.stderr)
        sys.exit(1)
    print(f'PASS preflight: 320x180 RGBA8; {count} palette colors; SHA256 {digest}')
    print('NOT art approval, rights clearance, GitHub binary verification or Unity import')
