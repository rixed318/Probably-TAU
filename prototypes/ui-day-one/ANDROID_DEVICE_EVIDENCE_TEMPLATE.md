# Android Day01 — physical device evidence form (#20)

**Status: NOT TESTED.** This is an empty evidence form, not a claim of passing Android QA. Test the downloadable `day01-android-touch.html` from the `feature/ui-5` workflow artifact; retain the older offline HTML separately. This is a browser prototype, not Unity or an APK.

## Reproducibility

- GitHub Actions run URL / artifact name: TODO
- Artifact commit SHA (must match tested branch): TODO
- File name and SHA-256 of extracted HTML: TODO
- Device / Android version / browser version: TODO
- CSS viewport width × height and device pixel ratio: TODO
- Android display density (dpi or density scale) and accessibility font/display scale: TODO
- Measured minimum touch target width × height in CSS px, and method used (DevTools / screenshot / ruler): TODO
- Test date and tester: TODO
- Network: airplane mode ON / OFF (circle one)

## Execute and record (PASS / FAIL / BLOCKED; never prefill PASS)

| ID | Steps | Expected | Result | Evidence URL / screenshot | Notes |
| --- | --- | --- | --- | --- | --- |
| A1 | Extract and open HTML from local storage with airplane mode enabled; reload | Day01 UI renders without network | TODO | TODO | TODO |
| A2 | Landscape ~800×360: visit DOC-01, inspect, decide; repeat DOC-02 and DOC-03 | Three distinct visitors, accessible buttons, no accidental double decision | TODO | TODO | TODO |
| A3 | End day after all three decisions | One report, coherent mock totals, report heading receives focus | TODO | TODO | TODO |
| A4 | Portrait ~360×800: repeat inspection and decisions | No clipped actions, reachable vertical scroll | TODO | TODO | TODO |
| A5 | Rotate with report open and zoom text to 200% | Content remains reachable; no horizontal scrolling trap | TODO | TODO | TODO |
| A6 | Enable TalkBack; navigate queue, errors, decision and report | Spoken labels and status, logical focus order | TODO | TODO | TODO |
| A7 | Notched device: both orientations; tap outer edge of each action | No cutout/gesture overlap, record measured CSS px and Android dp separately; target ≥48 dp in both dimensions | TODO | TODO | TODO |

## Touch-target measurement note

`48 CSS px` is a **source-level approximation**, not proof of `48 Android dp` on a physical device. Record the actual browser viewport, Android display scaling and measured interactive bounding rectangles. A CSS declaration such as `min-height:48px` is not a measured hit area, and a screenshot alone cannot prove accessibility. If dp conversion or measurement is unavailable, mark A7 **BLOCKED**, not PASS. Check actual clickable areas at the cutout and gesture-navigation edges in both orientations.

## Defect record (one block per failure)

- Case ID: TODO
- Exact steps to reproduce: TODO
- Expected / observed: TODO
- Screenshot/video and timestamp: TODO
- Browser console output (if available): TODO
- Severity / proposed owner: TODO

## Release gate

A static Node test or desktop emulation cannot substitute for these device results. Mark Issue #20 physical acceptance complete only after evidence is attached, reviewed by QA 05 and owner 00. Do not claim native APK, Unity Editor, or TalkBack verification from this form.
