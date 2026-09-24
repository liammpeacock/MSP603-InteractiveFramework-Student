# Getting started

## Before you begin

Use the approved **MSP603 v1.1.0 Student FMOD** package supplied through the VLE or the Student release surface. Do not use GitHub's automatically generated source archives.

The Student package includes the playable Unity project, FMOD for Unity integration, a matching Student FMOD Studio project, the required event/parameter/bank structure, and compiled reference banks.

## Open the Unity project

1. Extract the package to a normal writable working folder; do not run it from inside the ZIP.
2. In Unity Hub, add the extracted project folder.
3. Open it in Unity `6000.0.78f1` and allow the first import to complete.
4. Open `Assets/MSP603/Scenes/MainMenu.unity` and enter Play Mode.
5. Select **Play Level 1** to confirm the gameplay project opens with 20 lives and offers Cannon, Archer, Mage, and Inferno on a build pad.
6. Before changing the FMOD authoring project, confirm that the supplied compiled reference banks produce the expected reference audio.

![Unity Level 1 running with enemies and placed towers visible](images/unity-gameplay-level01.png)

*Level 1 provides a quick gameplay and audio-integration check.*

## Open the supplied Student FMOD project

The matching Student project is:

`Audio/FMOD/MSP603_26-27_TD/MSP603_26-27_TD.fspro`

Open this supplied project in FMOD Studio `2.03.14`. Do not replace it with an unrelated new FMOD project.

![Student FMOD Event Browser showing the supplied Enemies, Music, Towers and UI event scaffold](images/fmod-student-event-scaffold.png)

*The supplied Student FMOD project retains the event structure required by the framework.*

## What is supplied and what remains yours

The Student FMOD project is a **scaffold, not a completed sound-design solution**. It retains the integration contract needed by Unity: event paths and identities, required parameters, bank structure and supporting project metadata. Lecturer source audio and the completed Lecturer sonic implementation are not supplied.

You author and implement your own audio, parameter behaviour, event content, spatial response, routing, mixing and creative decisions as required by the assessment. Some routing is deliberately incomplete so that you must understand and implement it yourself.

The compiled reference banks are included as a known-good diagnostic starting point. They let you prove that the Unity framework works before you begin replacing the reference behaviour with your own FMOD work.

Continue with [Setting Up FMOD After Downloading the Project](FMODSetupGuide.md).
