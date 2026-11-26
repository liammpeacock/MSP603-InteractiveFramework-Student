# Student v2.0.0 — release notes draft

**Unreleased; no v2 ZIP or checksum is available yet.**

## New Features

Four global continuous/game-controlled counts: EnemyCount, RunnerCount, GruntCount and ArmouredCount, each independently 0–60/default 0, with lifecycle resets. Pause-state and ambience-emitter lifecycle, Run In Background/Live Update support, and a 15% effective tower-range increase are retained.

## Bug Fixes

Input System UI and assembly references, settings/pause lifecycle, and Lives publication before LifeLost playback. Lives resets to 20 for a level/restart. This supports parameter-conditioned countdowns and does not reset Sequential Global Scope playlist history.

## Audio-free authoring boundary

The intended package retains the 24 supplied event structures/identities, parameter definitions, mixer routing, Unity event assignments and matching freshly built banks. It contains **no authored audio in either source or compiled banks**. Music, effects, footsteps and pause ambience are intentionally silent until students author and rebuild their audio. No private Canonical/Lecturer/Student-Dev audio or old reference banks are approved for this public package.

## Compatibility and installation

Unity 6000.0.78f1, Studio 2.03.13 and included FMOD for Unity 2.03.13. Keep Unity and Audio adjacent. Follow [setup](FMODSetupGuide.md), [software](RequiredSoftware.md) and [migration/backups](StudentMigration.md). Do not upgrade an existing assessment project in place.

## Verification and known limitations

Source preparation and privacy/export regression checks pass. Final Studio round-trip, six new silent banks, native source/bank checks, extracted package checks, complete PlayMode and runtime acceptance are pending. This draft is not evidence of a release pass.

## Downloads

The [original v1.1.0 release](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/tag/v1.1.0) and [fixed v1.1.0 ZIP](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/download/v1.1.0/MSP603-InteractiveFramework-Student-v1.1.0.zip) remain unchanged. Add direct v2 ZIP/checksum links only after verified publication. A VLE `/releases/latest` link may later follow v2; use the fixed v1.1.0 URL when that original version is required.
