"""Offline regression tests for the candidate-only PNG audit; requires Pillow."""
import hashlib
import tempfile
import unittest
from pathlib import Path

from PIL import Image

from audit_candidate import audit


class CandidateAuditTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self.tmp.cleanup)
        self.path = Path(self.tmp.name) / 'candidate.png'

    def save(self, mode='RGB', size=(320, 180), color=(24, 57, 47)):
        Image.new(mode, size, color).save(self.path)
        return audit(self.path)

    def test_native_grid_rgb_and_sha(self):
        result = self.save()
        self.assertEqual(result['dimensions'], [320, 180])
        self.assertEqual(result['mode'], 'RGB')
        self.assertEqual(result['unique_colors'], 1)
        self.assertEqual(result['top_colors'][0]['pixels'], 57600)
        self.assertEqual(result['sha256'], hashlib.sha256(self.path.read_bytes()).hexdigest())
        self.assertTrue(result['native_grid_320x180'])
        self.assertTrue(result['within_24_colors'])
        self.assertFalse(result['production_ready'])

    def test_wrong_dimensions_are_reported_not_approved(self):
        result = self.save(size=(319, 180))
        self.assertFalse(result['native_grid_320x180'])
        self.assertFalse(result['production_ready'])

    def test_rgba_candidate_supported(self):
        result = self.save(mode='RGBA', color=(24, 57, 47, 128))
        self.assertEqual(result['mode'], 'RGBA')
        self.assertEqual(result['top_colors'][0]['rgb'], '#18392F')
        self.assertFalse(result['production_ready'])

    def test_over_24_colors_flagged(self):
        image = Image.new('RGB', (320, 180))
        for i in range(25):
            image.putpixel((i, 0), (i, 0, 0))
        image.save(self.path)
        result = audit(self.path)
        self.assertEqual(result['unique_colors'], 25)
        self.assertFalse(result['within_24_colors'])

    def test_non_png_rejected(self):
        Image.new('RGB', (320, 180)).save(self.path, format='JPEG')
        with self.assertRaisesRegex(ValueError, 'Expected PNG'):
            audit(self.path)

    def test_palette_mode_rejected(self):
        Image.new('P', (320, 180)).save(self.path)
        with self.assertRaisesRegex(ValueError, 'Expected RGB/RGBA'):
            audit(self.path)


if __name__ == '__main__':
    unittest.main()
