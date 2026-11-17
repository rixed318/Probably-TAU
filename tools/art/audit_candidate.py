#!/usr/bin/env python3
"""Audit an unapproved pixel-art PNG candidate (requires Pillow)."""
import argparse
import hashlib
import json
from collections import Counter
from pathlib import Path
from PIL import Image


def audit(path: Path) -> dict:
    raw = path.read_bytes()
    with Image.open(path) as im:
        im.load()
        if im.format != 'PNG':
            raise ValueError('Expected PNG')
        if im.mode not in ('RGB', 'RGBA'):
            raise ValueError(f'Expected RGB/RGBA PNG, got {im.mode}')
        pixels = Counter(im.get_flattened_data())
        return {
            'file': path.name,
            'sha256': hashlib.sha256(raw).hexdigest(),
            'bytes': len(raw),
            'dimensions': list(im.size),
            'mode': im.mode,
            'unique_colors': len(pixels),
            'top_colors': [{'rgb': '#'+''.join(f'{c:02X}' for c in rgb[:3]), 'pixels': count}
                           for rgb, count in pixels.most_common(3)],
            'native_grid_320x180': im.size == (320, 180),
            'within_24_colors': len(pixels) <= 24,
            'production_ready': False,
        }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('png', type=Path)
    args = parser.parse_args()
    print(json.dumps(audit(args.png), ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
