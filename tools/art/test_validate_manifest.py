#!/usr/bin/env python3
"""Offline regression tests for the Day 01 art manifest validator (Issue #13)."""
from __future__ import annotations

import csv
import importlib.util
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MODULE_PATH = ROOT / "tools/art/validate_manifest.py"
spec = importlib.util.spec_from_file_location("validate_manifest", MODULE_PATH)
validator = importlib.util.module_from_spec(spec)
assert spec and spec.loader
spec.loader.exec_module(validator)


class ManifestValidationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.path = Path(self.temp.name) / "manifest.csv"
        with validator.MANIFEST.open(encoding="utf-8-sig", newline="") as f:
            self.rows = list(csv.DictReader(f))
        self.assertGreater(len(self.rows), 0, "Test fixture requires current manifest")

    def check(self, expected=None):
        with self.path.open("w", encoding="utf-8", newline="") as f:
            writer = csv.DictWriter(f, fieldnames=validator.FIELDS)
            writer.writeheader()
            writer.writerows(self.rows)
        errors = validator.validate(self.path)
        if expected is None:
            self.assertEqual(errors, [])
        else:
            self.assertTrue(any(expected in e for e in errors), errors)

    def test_repository_manifest_is_valid(self):
        self.check()

    def test_duplicate_asset_id_rejected(self):
        self.rows[1]["asset_id"] = self.rows[0]["asset_id"]
        self.check("Duplicate asset ID")

    def test_missing_critical_asset_rejected(self):
        self.rows = [r for r in self.rows if r["asset_id"] != "office_wall"]
        self.check("Missing critical first-day asset: office_wall")

    def test_wrong_world_resolution_rejected(self):
        self.rows[0]["native_px"] = "321x180"
        self.check("world layers must be 320x180")

    def test_out_of_bounds_anchor_rejected(self):
        self.rows[0]["anchor_px"] = "320,0"
        self.check("invalid native size / anchor / frame count")

    def test_unapproved_asset_provenance_rejected(self):
        self.rows[0]["status"] = "approved"
        self.check("approved asset missing source provenance")

    def test_unauthorized_unity_target_rejected(self):
        self.rows[0]["unity_target"] = "Assets/External/office_wall.png"
        self.check("Unity target outside proposed art directory")

    def test_invalid_frame_count_rejected(self):
        self.rows[0]["frames"] = "0"
        self.check("invalid native size / anchor / frame count")


if __name__ == "__main__":
    unittest.main()
