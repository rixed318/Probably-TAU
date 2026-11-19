# QA 05 — Android Unity build/signing acceptance (Issues #8, #20)

Status: PREPARATION ONLY. This is not evidence of an APK or device run; no architecture contract is approved.

## Source and art
- [ ] Record exact full 40-character commit SHA of Core #9, NPC #16, UI #12, Content #11, QA #10 and integration commit.
- [ ] Respect C0-v2 and ADR #14; do not merge without owner approval.
- [ ] Verify real Unity 6 project with ProjectSettings/ProjectVersion.txt, Assets and Packages/manifest.json; record Editor version.
- [ ] Verify 3–5 actual original PNG binaries, 8-byte PNG signatures, IHDR dimensions, SHA-256 and corresponding manifest hashes; planned rows are not binary evidence.
- [ ] Do not assume third-party IP/logo permission.

## Unity Android build (NOT RUN until Editor available)
- [ ] Install Unity 6 Editor and Android Build Support with SDK, NDK and OpenJDK; record versions.
- [ ] Switch to Android target and record portrait/landscape settings, package identifier and scripting backend.
- [ ] Compile/import and record warnings/errors; build a real development APK from an exact integration SHA.
- [ ] Record APK path, SHA-256, bytes, build timestamp, Unity version, source SHA and build logs.
- [ ] Inspect APK metadata using aapt or apkanalyzer; verify signing with apksigner when available.
- [ ] Never commit .keystore, .jks, passwords, key.properties or SDK credentials. Use owner-managed signing and approved CI secrets only; release signing is out of scope.

## Mobile acceptance
- [ ] In Chromium touch emulation open the exact latest single-file Day01 HTML at 360x800 portrait and 800x360 landscape; record exact UI SHA and browser version.
- [ ] In each orientation verify three documents, decisions, EndDay, ledger replay, no immediate partner cash, 48dp touch targets, horizontal overflow and clipped text.
- [ ] For actual Android APK separately record device model, OS, scaling and orientation; adb install -r, launch, logcat, screenshots and observed results.
- [ ] On real APK verify three documents, EndDay, ledger idempotency, no immediate partner cash and 48dp targets. Never infer APK PASS from Chromium.
- [ ] Save→Load and restart/replay require stable Core DTO, deterministic RNG and real executable tests.
- [ ] For every gate report PASS/FAIL/NOT RUN with reproduction steps, exact SHA, logs and evidence in PR #10.

## CI and safety
- [ ] Pin immutable exact SHAs, verify checked-out commit matches expected; moving branch tests are not exact-SHA tests.
- [ ] QA jobs use read-only contents permissions, no persisted checkout credentials, bounded timeouts and no repository writes.
- [ ] No automatic signing, secret upload, merge or main push by QA.

Current verdict (2026-11-19): checklist created. Chromium, Unity Editor, APK, Android device, PNG binary verification and integrated Save→Load are NOT RUN in this task.
