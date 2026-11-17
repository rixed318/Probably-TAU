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
        # Fully transparent RGB is invisible; it must not consume visible palette slots.
        visible = Counter({color: count for color, count in pixels.items()
                           if len(color) == 3 or color[3] != 0})
        partial_alpha = any(len(color) == 4 and 0 < color[3] < 255 for color in pixels)
        return {
            'file': path.name,
            'sha256': hashlib.sha256(raw).hexdigest(),
            'bytes': len(raw),
            'dimensions': list(im.size),
            'mode': im.mode,
            'alpha_pixels': sum(count for rgba, count in pixels.items() if len(rgba) == 4 and rgba[3] < 255),
            'has_partial_alpha': partial_alpha,
            'unique_colors': len(visible),
            'top_colors': [{'rgb': '#'+''.join(f'{c:02X}' for c in rgb[:3]), 'pixels': count}
                           for rgb, count in visible.most_common(3)],
            'native_grid_320x180': im.size == (320, 180),
            'within_24_colors': len(visible) <= 24,
            'production_ready': False,
        }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('png', type=Path)
    args = parser.parse_args()
    print(json.dumps(audit(args.png), ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
