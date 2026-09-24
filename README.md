# MSP603 v1.1.0 — Student FMOD

This is the student-facing release and documentation surface for the MSP603 Interactive Framework. It contains the playable Unity framework, FMOD for Unity integration, a matching Student FMOD Studio project, required event/parameter/bank structures, and known-good compiled reference banks. It does not contain the Lecturer source audio or completed reference sound design.

## 🎮 Play the reference game

**[Play MSP603 Tower Defence in your browser](https://bimmmcrliampeacock.itch.io/msp603-tower-defence)** — no installation is required.

Use the browser version to learn the game, explore all three levels, and understand the gameplay states before or alongside your own audio implementation. Your assessed audio work is created and tested in the approved Unity/FMOD Student project.

## Required software

- Unity `6000.0.78f1`
- FMOD Studio `2.03.14`
- FMOD for Unity `2.03.14`

Wwise is not included in v1.1.0.

## Start here

Begin with the [MSP603 Student Guide](docs/StudentGuide.md) and [Getting Started](docs/GettingStarted.md). The [documentation index](docs/README.md) provides the complete set of individually linkable Canvas/VLE topics.

The supplied Student FMOD project is a scaffold, not a completed solution. Preserve its project identity, event paths, parameters and bank structure. You author the audio, parameter behaviour, routing, mixing, spatial response and creative implementation required by the assessment. Mixer/event routing is deliberately incomplete where it forms part of the learning activity.

The package also includes known-good compiled reference banks. These let you confirm that the Unity framework and integration work before replacing the reference behaviour with your own authored and rebuilt FMOD banks.

## v1.1.0 teaching features

v1.1.0 adds teaching support for parameter-driven pause behaviour and per-tower cursor proximity, keeps Unity running in the background for supported FMOD Live Update workflows, and retains the existing three-level tower-defence framework.

For setup, recovery and the supplied `.fspro` workflow, use [Setting Up FMOD After Downloading the Project](docs/FMODSetupGuide.md) and [Troubleshooting and Support](docs/Troubleshooting.md).
