# MSP603 Interactive Framework — v1.1.0

## Release summary

v1.1.0 expands the MSP603 Interactive Framework for teaching and assessment delivery while retaining the established tower-defence gameplay framework.

The Student distribution now includes a matching FMOD Studio project scaffold alongside the Unity project. The scaffold preserves the integration architecture required by Unity while deliberately leaving assessed sound-design and routing decisions for students to implement.

## Key changes

- Added matching Student FMOD Studio project/template.
- Preserved the Unity/FMOD event, parameter and bank integration contract.
- Retained known-good reference banks for diagnostic and initial playback purposes.
- Removed Lecturer source audio and completed sound-design content from the Student FMOD project.
- Left appropriate mixer/event routing incomplete as an intentional teaching and assessment activity.
- Added global `Paused` parameter support.
- Added local continuous `Proximity` parameter support for towers.
- Enabled the framework for FMOD Live Update teaching workflows.
- Added pause-menu ambience authoring opportunity.
- Increased tower effective ranges by 15%.
- Restored the Level 3 player-life-loss audio hook.
- Fixed the Input System UI regression.
- Updated Student and Lecturer documentation for the v1.1.0 workflow.
- Added clearer guidance distinguishing 2D audio, FMOD 3D spatialisation and parameter-driven spatial response.

## Student workflow

Students should use the supplied Unity project together with the matching Student FMOD Studio project.

The supplied FMOD project is a scaffold rather than a completed sonic implementation. Students are expected to author their own audio, routing and parameter responses within the supplied framework.

Lecturers may use separate clean FMOD projects when teaching concepts from first principles before students apply those concepts within the assessment framework.

## Compatibility

- Unity: 6000.0.78f1
- FMOD Studio: 2.03.14
- FMOD for Unity: 2.03.14

## Release boundary

v1.1.0 is intended to provide a stable framework from which students can complete the intended assessment and lecturers can deliver and support it reliably.

Further non-essential development should be deferred to a later release.
