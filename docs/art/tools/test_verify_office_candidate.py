#!/usr/bin/env python3
"""Standalone regression tests for ART-A0.1 office PNG preflight."""
import importlib.util
import tempfile
import unittest
from pathlib import Path
from PIL import Image

MODULE = Path(__file__).with_name('verify_office_candidate.py')
spec = importlib.util.spec_from_file_location('office_validator', MODULE)
validator = importlib.util.module_from_spec(spec)
spec.loader.exec_module(validator)


class OfficeCandidateTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.path = Path(self.temp.name) / 'office.png'

    def save(self, size=(320, 180), color=(24, 57, 47, 255), mode='RGBA'):
        Image.new(mode, size, color).save(self.path)

    def test_valid_opaque_palette_png(self):
        self.save()
        count, digest = validator.verify(self.path)
        self.assertEqual(count, 1)
        self.assertEqual(len(digest), 64)

    def test_wrong_dimensions_rejected(self):
        self.save(size=(319, 180))
        with self.assertRaisesRegex(ValueError, '320x180'):
            validator.verify(self.path)

    def test_transparency_rejected(self):
        self.save(color=(24, 57, 47, 0))
        with self.assertRaisesRegex(ValueError, 'opaque'):
            validator.verify(self.path)

    def test_outside_palette_rejected(self):
        self.save(color=(1, 2, 3, 255))
        with self.assertRaisesRegex(ValueError, 'palette'):
            validator.verify(self.path)

    def test_rgb_png_rejected(self):
        self.save(color=(24, 57, 47), mode='RGB')
        with self.assertRaisesRegex(ValueError, 'RGBA8'):
            validator.verify(self.path)

    def test_non_png_rejected(self):
        self.path.write_bytes(b'not a png')
        with self.assertRaisesRegex(ValueError, 'signature'):
            validator.verify(self.path)


if __name__ == '__main__':
    unittest.main()
