# MSP603 Interactive Framework v2.0.0

**Student edition · Released 26 November 2026**

## Overview and purpose

A playable Unity tower-defence framework for learning and assessing interactive audio with FMOD. Three levels provide gameplay events, changing enemy populations, tower interactions and a pause/settings interface against which you can design and test your own sound.

**Student v2.0.0 is an audio-free authoring package.** Unity gameplay works, but music, footsteps, effects and pause ambience are intentionally silent until you add your own audio and rebuild the banks. Existing students should use the version specified by their assessment brief; v2.0.0 is an optional upgrade.

## Downloads

- **[Download Student v2.0.0](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/download/v2.0.0/MSP603-Student-v2.0.0.zip)** — 296,410,829 bytes.
- [Release notes and all assets](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/tag/v2.0.0) · [SHA-256 checksum](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/download/v2.0.0/MSP603-Student-v2.0.0.zip.sha256) · [Verification summary](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/download/v2.0.0/MSP603-Student-v2.0.0.verification.json).
- [Historical v1.1.0 release](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/tag/v1.1.0) · [v1.0.0 release](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/tag/v1.0.0). Original tags and downloads remain unchanged.

## What’s new in v2.0.0

v2.0.0 adds independent enemy-population inputs, repairs integration and routing, and delivers tested, matching project/bank packages. The Student package now has a fully audio-free source-and-bank baseline.

### New features

- Four independent enemy-count parameters provide encounter-wide information to FMOD.
- Pause-state publication and pause-ambience event lifecycle support a separate pause soundscape.
- Run In Background and FMOD Live Update support inspecting parameters while switching between Unity and Studio.
- Matching event identities, parameter definitions, mixer routes and bank assignments provide a consistent Unity/FMOD starting point.

### Your audio authoring starting point

The supplied FMOD project preserves **24 event identities**, event structures, parameters, mixer routing, Unity assignments and **six matching rebuilt banks**. Neither the project nor the banks contains authored audio. Add your own appropriately licensed audio while preserving the integration structure.

Unlike v1.1.0’s diagnostic reference banks, v2.0.0’s banks are silent. Expected silence is not an integration failure. Missing events, missing banks, missing parameters or Unity Console errors still require investigation. v2 also supplies functional routing, so the incomplete-routing exercise described in v1.1.0 documentation no longer describes this package.

### Bug fixes

- Corrected Input System UI and assembly configuration so the supplied interface works with the project’s input setup.
- Published the updated `Lives` value **before** starting `LifeLost`; lives initialize and reset to **20**.
- Corrected the `EnemyCount` parameter name while preserving its identity.
- Repaired five SFX event outputs so they follow Gameplay/SFX routing, pause attenuation and volume controls.
- Corrected the editor startup shortcut so it no longer replaces Unity Test Runner’s generated scene; regression fixtures now use the actual gameplay UI and restore user preferences.

### Gameplay improvements

- Tower effective range is 15% above the earlier baseline, including upgrade increments and level-specific bonuses. This carries forward the intended v1.1.0 range improvement; it is **not** another cumulative 15% increase over that release.
- Pause/resume handles gameplay attenuation and the pause-ambience lifecycle, while UI remains outside Gameplay attenuation.
- Enemy counts and lives return to their expected starting values when levels or runs restart.
- The three-level tower-defence framework retains its established four-wave baseline. Damage, costs, enemy speed, rewards and level definitions were not rebalanced in this release.

## FMOD parameters and applications

- **`EnemyCount`** — total active enemies; use it to vary musical intensity or the density of a combat soundscape.
- **`RunnerCount`** — active runners; use it to introduce urgency or a rhythmic layer as fast enemies accumulate.
- **`GruntCount`** — active grunts; use it to shape the weight or texture of an encounter.
- **`ArmouredCount`** — active armoured enemies; use it to introduce a heavier musical layer or threat cue.

All four are **global, continuous, game-controlled** parameters with a range of **0–60** and a default of **0**. Each count is published and clamped independently, updates as enemies enter or leave play, and resets with the gameplay lifecycle. The examples above are authoring possibilities, not additional pre-authored musical behaviors.

The existing **`Paused`** global supports Gameplay attenuation and pause-state authoring. Local **`Proximity`** supports tower-distance responses; it is parameter-driven behavior, not automatic 3D spatialisation. The existing **`Lives`** value is available before the life-loss event starts.

## Compatibility and installation

1. Download **[MSP603-Student-v2.0.0.zip](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/download/v2.0.0/MSP603-Student-v2.0.0.zip)** from the release assets and compare it with the published [SHA-256](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/download/v2.0.0/MSP603-Student-v2.0.0.zip.sha256). Use the named ZIP rather than GitHub’s generated “Source code” archive.
2. Back up existing work and extract into a **new, writable local folder**. Keep `Unity` and `Audio` together; do not overwrite an assessment project.
3. In Unity Hub, add `Unity/MSP603_26-27` and open it with **Unity 6000.0.78f1**. Allow package resolution and import to finish, then open `Assets/MSP603/Scenes/MainMenu.unity`.
4. Open `Audio/FMOD/Student/Student.fspro` in **FMOD Studio 2.03.13**, the v2 authoring baseline.
5. Use the included **FMOD for Unity/native runtime 2.03.13**. The release includes matching Desktop banks; no initial bank rebuild is required. After authoring changes, build Desktop banks and let Unity refresh them using the supplied relative paths.
6. Test Main Menu and all three levels, volume controls, pause/resume, parameter changes and resets. Add and build your own audio before expecting sound.

Do not import a second FMOD integration into the project. The Student package’s empty banks were generated with Studio **2.03.14** and validated against the included **2.03.13** native runtime; the project’s integration was not upgraded. v1.1.0 retains its separate documented 2.03.14 requirements.

## Verification status

The **final published ZIP** was extracted and tested: **22 EditMode and 55 PlayMode tests passed**, with **zero failures or skips**. Validation also checked native FMOD **2.03.13** bank loading, all **103 serialized event assignments**, required parameter contracts and every payload hash. GitHub’s uploaded sizes and SHA-256 digests match the tested files.

Audio-free checks confirmed no source audio or embedded waveform samples in the supplied banks. The public ZIP was successfully downloaded without authentication. Student silence is intentional; automated integration checks do not represent audible acceptance of audio you subsequently author.

## Known limitations

- Fresh v2.0.0 runtime verification covers **macOS Apple silicon**. A new Windows runtime acceptance pass was not performed; historical Windows validation should not be read as v2 certification.
- The existing browser build was not updated or revalidated for v2.0.0. Use the versioned Unity authoring ZIP for this release.
- Publishing `Lives` before `LifeLost` supports parameter-conditioned selection, but does not reset **Sequential Global Scope** playlist history. Use parameter-conditioned instruments where a deterministic life-count response is required.
- FMOD is the supplied middleware; Wwise is not included.

## Documentation and release history

[Getting started](docs/GettingStarted.md) · [FMOD setup](docs/FMODSetupGuide.md) · [Migration and backups](docs/StudentMigration.md) · **[Comprehensive changelog](CHANGELOG.md)**.

Repository documentation is maintained after publication. Documentation inside a release ZIP remains that release’s snapshot; documentation-only updates do not replace the tested ZIP or move its tag.
