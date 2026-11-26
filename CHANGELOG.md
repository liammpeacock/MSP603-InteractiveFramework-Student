# Changelog

Release history for the **Student edition** of MSP603 Interactive Framework. Newest releases appear first. Historical entries describe their own release requirements and behavior; use the v2.0.0 instructions for the current package.

## [2.0.0] — 2026-11-26

### Release overview

v2.0.0 adds independent enemy-population inputs, repairs integration and routing, and delivers tested, matching project/bank packages. The Student package now has a fully audio-free source-and-bank baseline.

### Added

- Four independent enemy-count parameters provide encounter-wide information to FMOD.
- Pause-state publication and pause-ambience event lifecycle support a separate pause soundscape.
- Run In Background and FMOD Live Update support inspecting parameters while switching between Unity and Studio.
- Matching event identities, parameter definitions, mixer routes and bank assignments provide a consistent Unity/FMOD starting point.

### FMOD parameters

- **`EnemyCount`** — total active enemies; use it to vary musical intensity or the density of a combat soundscape.
- **`RunnerCount`** — active runners; use it to introduce urgency or a rhythmic layer as fast enemies accumulate.
- **`GruntCount`** — active grunts; use it to shape the weight or texture of an encounter.
- **`ArmouredCount`** — active armoured enemies; use it to introduce a heavier musical layer or threat cue.

All four are **global, continuous, game-controlled** parameters with a range of **0–60** and a default of **0**. Each count is published and clamped independently, updates as enemies enter or leave play, and resets with the gameplay lifecycle. The examples above are authoring possibilities, not additional pre-authored musical behaviors.

The existing **`Paused`** global supports Gameplay attenuation and pause-state authoring. Local **`Proximity`** supports tower-distance responses; it is parameter-driven behavior, not automatic 3D spatialisation. The existing **`Lives`** value is available before the life-loss event starts.

### Fixed

- Corrected Input System UI and assembly configuration so the supplied interface works with the project’s input setup.
- Published the updated `Lives` value **before** starting `LifeLost`; lives initialize and reset to **20**.
- Corrected the `EnemyCount` parameter name while preserving its identity.
- Repaired five SFX event outputs so they follow Gameplay/SFX routing, pause attenuation and volume controls.
- Corrected the editor startup shortcut so it no longer replaces Unity Test Runner’s generated scene; regression fixtures now use the actual gameplay UI and restore user preferences.

### Gameplay

- Tower effective range is 15% above the earlier baseline, including upgrade increments and level-specific bonuses. This carries forward the intended v1.1.0 range improvement; it is **not** another cumulative 15% increase over that release.
- Pause/resume handles gameplay attenuation and the pause-ambience lifecycle, while UI remains outside Gameplay attenuation.
- Enemy counts and lives return to their expected starting values when levels or runs restart.
- The three-level tower-defence framework retains its established four-wave baseline. Damage, costs, enemy speed, rewards and level definitions were not rebalanced in this release.

### Audio-free authoring package

The supplied FMOD project preserves **24 event identities**, event structures, parameters, mixer routing, Unity assignments and **six matching rebuilt banks**. Neither the project nor the banks contains authored audio. Add your own appropriately licensed audio while preserving the integration structure.

Unlike v1.1.0’s diagnostic reference banks, v2.0.0’s banks are silent. Expected silence is not an integration failure. Missing events, missing banks, missing parameters or Unity Console errors still require investigation. v2 also supplies functional routing, so the incomplete-routing exercise described in v1.1.0 documentation no longer describes this package.

### Compatibility and installation

Unity **6000.0.78f1**; FMOD Studio authoring baseline and included FMOD for Unity/native runtime **2.03.13**. Matching Desktop banks are included. The Student empty-bank build used Studio 2.03.14 and passed native 2.03.13 validation without an integration upgrade. Extract upgrades separately and retain existing work. See [installation](README.md#compatibility-and-installation).

### Verified

The **final published ZIP** was extracted and tested: **22 EditMode and 55 PlayMode tests passed**, with **zero failures or skips**. Validation also checked native FMOD **2.03.13** bank loading, all **103 serialized event assignments**, required parameter contracts and every payload hash. GitHub’s uploaded sizes and SHA-256 digests match the tested files.

Audio-free checks confirmed no source audio or embedded waveform samples in the supplied banks. The public ZIP was successfully downloaded without authentication. Student silence is intentional; automated integration checks do not represent audible acceptance of audio you subsequently author.

### Known limitations

- Fresh v2.0.0 runtime verification covers **macOS Apple silicon**. A new Windows runtime acceptance pass was not performed; historical Windows validation should not be read as v2 certification.
- The existing browser build was not updated or revalidated for v2.0.0. Use the versioned Unity authoring ZIP for this release.
- Publishing `Lives` before `LifeLost` supports parameter-conditioned selection, but does not reset **Sequential Global Scope** playlist history. Use parameter-conditioned instruments where a deterministic life-count response is required.
- FMOD is the supplied middleware; Wwise is not included.

### Availability

[Published v2.0.0 release](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/tag/v2.0.0) · [Versioned ZIP](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/download/v2.0.0/MSP603-Student-v2.0.0.zip) · [SHA-256](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/download/v2.0.0/MSP603-Student-v2.0.0.zip.sha256). All three v2 packages share accepted Canonical source `71626e1676b8416def539e300e3168e2a8c0206f`. The Student release tag points to `c43272f71a99a01ac869d688e5e9eb57296fa390`. Release assets and historical tags remain unchanged by subsequent documentation updates.

## [1.1.0] — 2026-09-24

### Added and changed

- Supplied a matching Student FMOD scaffold and corresponding Lecturer teaching reference.
- Added global `Paused`, local tower `Proximity`, pause-ambience authoring support and FMOD Live Update workflows.
- Increased effective tower ranges by 15%, restored the Level 3 life-loss hook and corrected the Input System UI regression.
- Expanded guidance on 2D audio, FMOD 3D spatialisation and parameter-driven spatial responses.
- The Student scaffold excluded completed source-audio work but shipped audible diagnostic reference banks and intentionally incomplete routing. This historical workflow differs from v2’s fully audio-free banks and repaired routing.

### Compatibility and preservation

- Unity **6000.0.78f1**, FMOD Studio and FMOD for Unity **2.03.14**, as documented for that release.
- [Original release and assets](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/tag/v1.1.0) remain unchanged. Existing assessments may continue to require this version.

## [1.0.0] — 2026-09-06

### Initial release

- Released the three-level Unity tower-defence framework for MSP603 audio authoring and teaching.
- Provided the public Student FMOD authoring package; blank event/bank assignments were part of the original authoring workflow.
- Required Unity **6000.0.78f1**, FMOD Studio and FMOD for Unity **2.03.14**.
- Historical validation covered macOS Apple silicon and Windows Intel/x64; this does not extend to fresh v2 Windows acceptance.
- FMOD was the supplied middleware; Wwise was not included.
- [Original release and assets](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/tag/v1.0.0) remain unchanged.
