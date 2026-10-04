# MSP603 Interactive Framework — v1.1.0

Version 1.1.0 expands the MSP603 FMOD teaching framework with new interactive-audio opportunities, improved live authoring support, a more reliable Student FMOD workflow, and targeted fixes identified through classroom testing.

This is an incremental development of the v1.0.0 framework. The core three-level tower-defence game and existing FMOD integration remain the foundation.

## What's new

### FMOD Live Update workflow

Unity is now configured to **Run In Background**.

This allows the game to continue running when switching from Unity to FMOD Studio and supports Live Update workflows during teaching, testing and authoring.

Students can use this to observe active events, parameters, mixer behaviour and other runtime information while the game continues to run.

### Pause-state authoring

The framework now exposes pause state to FMOD through the global labelled parameter:

`Paused`

States:

- `Unpaused`
- `Paused`

Unity reports the game state while the FMOD implementation determines the audible response.

A dedicated authoring opportunity is also provided:

`event:/UI/Pause_Menu_Ambience`

This supports teaching and experimentation with pause-state audio without prescribing a particular sonic solution.

### Per-tower Proximity

Tower Ambience now exposes the local continuous parameter:

`Proximity`

Range:

`0.0–1.0`

Each placed tower independently reports its relationship to the player's cursor.

This creates opportunities for parameter-driven interaction and provides a practical teaching example of the difference between:

- 2D playback;
- genuine FMOD 3D spatialisation;
- parameter-driven spatial response.

`Proximity` is not an additional FMOD listener and does not replace genuine 3D spatialisation.

### Tower interaction range

Effective tower ranges have been increased by **15%**.

The `Proximity` interaction system derives its radius from the tower's current Effective Range, so it follows the updated gameplay range automatically.

### Student FMOD authoring project

The Student distribution now includes a matching **Student FMOD Studio project**.

The project preserves the integration contract required by Unity, including the required event, parameter and bank identities.

It does not contain the Lecturer source audio or completed Lecturer sound-design solution.

Parts of the implementation, including mixer routing, are deliberately left for Student authoring.

This allows students to work within a reliable integration structure while retaining responsibility for their own creative and technical audio implementation.

### Known-good reference banks

The Student Unity project initially includes compiled reference banks containing known-good reference audio.

These provide a diagnostic starting point:

**Reference implementation works → framework is functioning → develop and test the Student FMOD implementation.**

Students replace the relevant reference behaviour as they author their own implementation and build their own banks.

### Improved FMOD recovery workflow

The supplied Student FMOD project now provides a matching project identity for the Unity integration.

This improves recovery when FMOD for Unity needs to be repaired or reimported and avoids relying on students creating an unrelated replacement FMOD project.

### Runtime UI input fix

Runtime UI now uses Unity's current Input System UI integration, resolving pointer/input problems encountered during testing.

### Level 3 life-loss audio fix

The missing Level 3 `Player Life Lost` event assignment has been restored.

Level 3 now uses:

`event:/UI/LifeLost`

consistently with the framework's intended life-loss behaviour.

## Documentation

Student documentation has been revised for v1.1.0 to explain:

- the supplied Student FMOD project;
- reference banks;
- the Student/Framework authoring boundary;
- Live Update;
- pause-state authoring;
- `Proximity`;
- local and global parameters;
- 2D, 3D and parameter-driven spatial behaviour;
- mixer routing;
- testing;
- recovery and troubleshooting.

The documentation also distinguishes between techniques learned from scratch during teaching and their application within the supplied assessment framework.

## Required software

- Unity `6000.0.78f1`
- FMOD Studio `2.03.14`
- FMOD for Unity `2.03.14`

## Compatibility

The Student Unity authoring project is intended for:

- macOS Apple silicon;
- Windows Intel/x64.

Final release validation should be completed using the approved v1.1.0 release package.

## Assessment boundary

The framework supplies gameplay systems, integration structures and interactive-audio opportunities.

It does not supply the Student's assessed creative response.

Supplied framework structures such as event identities, parameter identities, bank structure and Unity gameplay hooks should not be presented as Student-created work.

Students remain responsible for the creative and technical implementation required by the current assessment brief.

## Wwise

Wwise is **not included in v1.1.0**.

A Wwise version of the framework is a separate future development stream.
