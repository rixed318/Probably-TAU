#!/usr/bin/env python3
"""Validate the draft Day 01 art manifest. No Unity or third-party dependencies."""
from __future__ import annotations

import csv
import re
import sys
from collections import Counter
from pathlib import Path

MANIFEST = Path(__file__).resolve().parents[2] / "docs/art/ASSET_MANIFEST_D01.csv"
FIELDS = (
    "asset_id", "group", "purpose", "content_ref", "priority", "native_px",
    "anchor_px", "layer", "frames", "style_rev", "authoring_tool_model",
    "prompt_ref", "reference_id", "status", "rights_review", "source_file",
    "unity_target", "notes"
)
GROUPS = {"World", "Props", "Characters", "Documents", "UI", "Animation"}
PRIORITY = {"P0", "P1", "P2"}
PROMPTS = {"P_SCENE", "P_PROP", "P_CHARACTER", "P_PORTRAIT",
           "P_DOCUMENT", "P_ICON", "P_ANIMATION"}
STATUSES = {
    "planned", "candidate", "selected_for_cleanup", "qa_ready",
    "approved", "production", "rejected"
}
REQUIRED = {
    "office_wall", "office_floor", "office_window", "desk_body",
    "desk_top", "doc01_procurement_blank", "doc02_student_proposal_blank",
    "doc03_partner_letter_blank", "room_dispatch_notice_blank",
    "report_ledger_blank", "hud_inspection", "anim_folder_slide",
}
for npc in ("vera_lisina", "irina_korneeva", "dasha_polynskaya", "mark_sviridov"):
    REQUIRED.add(npc + "_standing")
    REQUIRED.add(npc + "_portrait_neutral")


def validate(path: Path) -> list[str]:
    errors: list[str] = []
    if not path.is_file():
        return [f"Manifest missing: {path}"]
    with path.open(encoding="utf-8-sig", newline="") as fh:
        reader = csv.DictReader(fh)
        if reader.fieldnames != list(FIELDS):
            return [f"Header mismatch: expected {', '.join(FIELDS)}"]
        rows = list(reader)
    if not rows:
        return ["Manifest has no entries"]
    ids: list[str] = []
    for line, row in enumerate(rows, 2):
        rid = row["asset_id"]
        ids.append(rid)
        if not re.fullmatch(r"[a-z][a-z0-9_]+", rid):
            errors.append(f"L{line}: invalid asset_id {rid!r}")
        if any(not row[f].strip() for f in ("purpose", "content_ref", "style_rev",
                                             "prompt_ref", "unity_target")):
            errors.append(f"L{line}: missing required values")
        if row["group"] not in GROUPS:
            errors.append(f"L{line}: unknown group {row['group']}")
        if row["priority"] not in PRIORITY:
            errors.append(f"L{line}: invalid priority")
        if row["prompt_ref"] not in PROMPTS:
            errors.append(f"L{line}: unknown prompt template {row['prompt_ref']}")
        if row["status"] not in STATUSES:
            errors.append(f"L{line}: invalid status {row['status']}")
        try:
            w, h = map(int, row["native_px"].split("x"))
            x, y = map(int, row["anchor_px"].split(","))
            if w <= 0 or h <= 0 or not (0 <= x < w and 0 <= y < h):
                raise ValueError
            frames = int(row["frames"])
            if frames < 1:
                raise ValueError
        except (ValueError, TypeError):
            errors.append(f"L{line}: invalid native size / anchor / frame count")
            continue
        if row["group"] == "World" and (w, h) != (320, 180):
            errors.append(f"L{line}: world layers must be 320x180")
        if row["group"] == "Animation" and frames < 2:
            errors.append(f"L{line}: animation requires multiple frames")
        if row["group"] != "Animation" and frames != 1:
            errors.append(f"L{line}: only animation rows use multi-frame sheets")
        if row["status"] in {"approved", "production"}:
            if row["authoring_tool_model"] == "UNASSIGNED" or row["reference_id"] == "MASTER_TBD":
                errors.append(f"L{line}: approved asset missing source provenance")
            if row["rights_review"] in {"pending", ""} or not row["source_file"]:
                errors.append(f"L{line}: approved asset needs rights and source file")
        if not row["unity_target"].startswith("Assets/_Project/Art/"):
            errors.append(f"L{line}: Unity target outside proposed art directory")
    for aid, n in Counter(ids).items():
        if n > 1:
            errors.append(f"Duplicate asset ID: {aid}")
    for aid in sorted(REQUIRED - set(ids)):
        errors.append(f"Missing critical first-day asset: {aid}")
    return errors


if __name__ == "__main__":
    errs = validate(MANIFEST)
    if errs:
        print("FAILED art manifest validation:")
        for item in errs:
            print(" -", item)
        sys.exit(1)
    with MANIFEST.open(encoding="utf-8-sig", newline="") as fh:
        manifest_rows = list(csv.DictReader(fh))
    categories = Counter(r["group"] for r in manifest_rows)
    print("OK: art manifest valid;", len(manifest_rows), "planned assets.")
    print("Groups:", ", ".join(f"{key}={categories[key]}" for key in sorted(categories)))
