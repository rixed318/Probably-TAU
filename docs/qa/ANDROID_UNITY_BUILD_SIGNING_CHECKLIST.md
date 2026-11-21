# QA 05 — Android Unity development-build and signing acceptance checklist

Status: **PREPARATION ONLY** (Issue #20). This is not evidence of a Unity Editor, compiled APK, or Android device test. Browser HTML prototype is a separate track.

## Provenance / exact source
- [ ] Record complete commit SHAs for Core #9, NPC #16, Content #11, UI #12, QA #10, and any integration branch; preserve an immutable manifest alongside the build.
- [ ] Verify approved C0-v2 and ADR-C0-001/#14 plus ADR-C0-002/#17; note any still-unmerged contract, without granting approval.
- [ ] Record Unity **6.3 LTS Editor** version and actual Android Build Support, SDK, NDK, OpenJDK versions from the machine that builds. Record OS, runner identity, build command and Unity logs.
- [ ] Confirm a real Unity project, Android target, scene, Core asmdef integration and successful Editor compile. Do not substitute the offline HTML prototype.

## Build and signing
- [ ] Use Android **development/debug** signing for a sideloadable test APK; do not claim Play Store readiness. Confirm package ID, versionCode, minimum Android version and orientation settings.
- [ ] Never commit keystore, passwords, private keys, service-account JSON, or signing credentials to the repository or CI artifacts. If custom signing is required, inject secrets from protected CI secrets or secure local environment and redact logs.
- [ ] Build in the verified Unity Editor environment, capture exit code, warnings/errors, exact source SHAs, artifact name, byte size and SHA-256 checksum. Fail the gate if the APK does not exist or is zero bytes.
- [ ] Verify APK package metadata and signing with installed Android SDK tools (e.g. `aapt dump badging`, `apksigner verify --verbose`); record exact command/output and tool versions. Do not invent a passing result.
- [ ] Publish only the test APK and non-secret build manifest/logs to a scoped CI artifact; never publish signing keys. Use exact-SHA checkouts rather than moving branch HEADs.

## Device smoke (only after APK exists)
- [ ] Record actual device model, Android version, screen resolution, install method, APK SHA-256 and whether the app launches offline.
- [ ] Inspect and decide all three Day01 documents; verify deterministic Int64-kopeck budget and metrics, L-204 conflict, no immediate partner cash.
- [ ] Verify EndDay exactly once, receipts/ledger replay, repeat-tap rejection and no double charges; explicitly label unsupported deferred phases.
- [ ] Check 360×800 portrait and 800×360 landscape where supported, safe areas, readable text, 48dp tap targets, overflow, rotate/background/resume and app relaunch.
- [ ] Exercise Save→Load/replay only if the approved Core DTO and actual SaveService are integrated; otherwise mark **NOT IMPLEMENTED / NOT RUN**.

## Release labels
- `WEB_DEMO_TESTABLE`: independent browser acceptance and artifact evidence only.
- `APK_BUILT`: actual Unity Editor build, APK checksum and signing verification evidence.
- `ANDROID_DEVICE_TESTED`: physical device install and smoke evidence.
- `READY_FOR_OWNER_TEST`: require verified launch path and exact-SHA acceptance evidence; never infer from a green static CI alone.

**Current result (2026-11-20):** all Unity Editor / APK / physical-device checks **NOT RUN**. This checklist defines evidence to collect; it approves no architecture or external IP.
