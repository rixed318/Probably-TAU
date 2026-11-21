#!/usr/bin/env python3
"""Independent offline QA oracle for Core PCG32-v1; not a C# runtime test."""
import argparse
import unittest
from pathlib import Path

MASK64 = (1 << 64) - 1
MASK32 = (1 << 32) - 1
MULTIPLIER = 6364136223846793005
REFERENCE = (0xA15C02B7, 0x7B47F409, 0xBA1D3330,
             0x83D2F293, 0xBFA4784B, 0xCBED606E)


class Oracle:
    def __init__(self, seed, sequence):
        self.state = 0
        self.increment = ((sequence << 1) | 1) & MASK64
        self.next()
        self.state = (self.state + seed) & MASK64
        self.next()

    @classmethod
    def restore(cls, state, increment):
        if not increment & 1:
            raise ValueError("increment must be odd")
        obj = cls.__new__(cls)
        obj.state, obj.increment = state & MASK64, increment & MASK64
        return obj

    def next(self):
        old = self.state
        self.state = (old * MULTIPLIER + self.increment) & MASK64
        x = (((old >> 18) ^ old) >> 27) & MASK32
        rot = old >> 59
        return ((x >> rot) | (x << ((-rot) & 31))) & MASK32

    def bounded(self, maximum):
        if not 1 <= maximum <= MASK32:
            raise ValueError("maximum out of range")
        threshold = ((-maximum) & MASK32) % maximum
        while True:
            value = self.next()
            if value >= threshold:
                return value % maximum


class OracleTests(unittest.TestCase):
    def test_reference_vector(self):
        rng = Oracle(42, 54)
        self.assertEqual(tuple(rng.next() for _ in REFERENCE), REFERENCE)

    def test_save_load_replay(self):
        rng = Oracle(2026, 17)
        for _ in range(17):
            rng.next()
        restored = Oracle.restore(rng.state, rng.increment)
        self.assertEqual([rng.next() for _ in range(256)],
                         [restored.next() for _ in range(256)])

    def test_bounded_and_edge_cases(self):
        rng = Oracle(42, 54)
        for maximum in (1, 2, 3, 7, 65535, MASK32):
            for _ in range(200):
                self.assertLess(rng.bounded(maximum), maximum)
        with self.assertRaises(ValueError):
            rng.bounded(0)
        with self.assertRaises(ValueError):
            Oracle.restore(0, 2)

    def test_core_source_contract(self):
        if not SOURCE:
            self.skipTest("Pass --core-source to validate C# source contract")
        source = Path(SOURCE).read_text(encoding="utf-8")
        patterns = (
            r'AlgorithmId\s*=\s*"pcg32-v1"',
            r'Multiplier\s*=\s*6364136223846793005UL',
            r'\(\(sequence\s*<<\s*1\)\s*\|\s*1UL\)',
            r'oldState\s*\*\s*Multiplier\s*\+\s*Increment',
            r'\(\(oldState\s*>>\s*18\)\s*\^\s*oldState\)\s*>>\s*27',
            r'oldState\s*>>\s*59',
            r'\(\(-rotation\)\s*&\s*31\)',
            r'unchecked\(0U\s*-\s*maxExclusive\)\s*%\s*maxExclusive',
            r'value\s*>=\s*threshold',
        )
        for pattern in patterns:
            with self.subTest(pattern=pattern):
                self.assertRegex(source, pattern)
        # Checked C# compilation must retain deliberate 64→32 truncation.
        # This regression caught Core PR #9's real .NET 8 failure in run 43630164245.
        # It is a source guard only; the Core .NET runner verifies semantics.
        self.assertRegex(
            source,
            r'unchecked\s*\(\s*\(uint\)\s*\(\s*\(\s*\(oldState\s*>>\s*18\)',
        )
        self.assertNotIn("UnityEngine", source)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--core-source", default=None)
    args, _ = parser.parse_known_args()
    SOURCE = args.core_source
    unittest.main(argv=["test_pcg32_contract.py"])
