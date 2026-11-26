# MSP603 Student v2.0.0

Optional audio-free upgrade. Existing students may keep **v1.1.0** as specified by their brief/VLE. Back up and extract separately; never overwrite assessment work.

- [v2.0.0 release](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/tag/v2.0.0).
- [Download v2.0.0 ZIP](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/download/v2.0.0/MSP603-Student-v2.0.0.zip) · [SHA-256](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/download/v2.0.0/MSP603-Student-v2.0.0.zip.sha256).
- [Original v1.1.0 release](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/tag/v1.1.0) · [unchanged v1.1.0 ZIP](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/download/v1.1.0/MSP603-InteractiveFramework-Student-v1.1.0.zip).

## New Features and Bug Fixes

Four global continuous/game-controlled counts: EnemyCount, RunnerCount, GruntCount and ArmouredCount, independently 0–60/default 0 with lifecycle resets. Pause lifecycle, Run In Background/Live Update, 15% effective tower-range increase, Input System/assembly corrections, SFX routing and Lives-before-LifeLost with initial/reset Lives=20. Lives ordering does not reset Sequential Global Scope playlist history.

## Audio-free authoring

The Unity game works; **silence is expected** until students author audio. All 24 event identities, required parameter/mixer structures, Unity assignments and six matching banks are included. No authored audio is present in FMOD source or compiled banks. Preserve the scaffold and add your own appropriately licensed audio; do not substitute private or historical reference banks.

## Installation and compatibility

Unity 6000.0.78f1; included FMOD for Unity/native runtime 2.03.13; Studio 2.03.13 authoring baseline. Empty banks were built using already-installed Studio 2.03.14, without changing source/integration, and validated with native 2.03.13. Extract into a new writable local folder, keep Unity and Audio adjacent, open `Unity/MSP603_26-27` in Unity Hub and `Audio/FMOD/Student/Student.fspro` in Studio. No GitHub account is required for public release downloads.

## Verification and limitations

Source and actual extracted Student package pass 22 EditMode and 55 PlayMode tests with zero failures/skips, native 2.03.13 event/parameter checks, full file hashes and audio-free content checks. Canonical audible acceptance was maintainer-confirmed; Student is intentionally silent. New verification covers macOS Apple silicon; fresh Windows runtime acceptance was not performed. Existing browser build is unchanged.

[Start here](docs/GettingStarted.md) · [FMOD setup](docs/FMODSetupGuide.md) · [release notes](docs/ReleaseNotes-v2.0.0.md) · [migration/backups](docs/StudentMigration.md). Canvas was not accessed; `/releases/latest` now selects v2, while the fixed v1.1.0 URL remains available.

Accepted Canonical revision: `71626e1676b8416def539e300e3168e2a8c0206f`.
