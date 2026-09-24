# MSP603 Student Guide

## Start here

The **MSP603 Interactive Framework v1.1.0** is a playable, three-level tower-defence vertical slice for MSP603 Game Sound & Music. It gives you a working game in which you can design, implement, test, and evaluate interactive audio without first building the gameplay systems yourself.

The four playable towers are **Cannon**, **Archer**, **Mage**, and **Inferno**. Across the three levels you can explore waves, enemy movement and combat, tower placement and upgrades, projectile impacts, Inferno's sustained flame, player-life loss, music and ambience states, victory/defeat, and interface/settings feedback.

## What is supplied

The Student distribution provides:

- the playable Unity framework and FMOD for Unity integration;
- official FMOD components and accessible authoring objects;
- a matching Student FMOD Studio project;
- the required event, parameter and bank structures used by the framework;
- compiled known-good reference banks so you can verify the framework before replacing the reference audio with your own work.

The supplied Student FMOD project is a **scaffold**. It does not contain the Lecturer source audio or completed reference sound design. Some implementation work, including mixer/event routing, is deliberately incomplete because understanding and completing it is part of the learning process.

Do not replace the supplied matching FMOD project with an unrelated project. Preserve the documented integration contract while developing your own work.

## What you author

You remain responsible for the assessed creative and technical work required by the current brief: your source audio, sound design, parameter behaviour, routing, mixing, spatial response, testing and creative decisions.

The framework publishes useful gameplay state rather than deciding the sonic result for you. For example, v1.1.0 exposes a global labelled `Paused` state and a local continuous `Proximity` value for each tower. You determine how those states affect your FMOD implementation.

A clean FMOD project may still be used during teaching to learn a technique from scratch. The assignment workflow is then to recognise the corresponding structure in the supplied Student project and apply your own implementation there.

The framework supports MSP603 learning and assessment activity; it does not replace the official module specification, current assessment brief, Canvas/VLE instructions, or your own assessed creative work.

## Recommended journey

1. [Download and open the project](GettingStarted.md).
2. [Check the required software](RequiredSoftware.md).
3. [Set up FMOD after downloading](FMODSetupGuide.md).
4. [Play and test the complete vertical slice](VerticalSlicePlayGuide.md).
5. [Find the safe authoring locations](ProjectNavigation.md).
6. Review the [FMOD authoring reference](FMODAuthoringReference.md).
7. Follow the [FMOD component workflow](FMODComponentWorkflow.md).
8. Use the [assessment authoring workflow](AssessmentAuthoring.md) alongside your current brief.

## Help and bug reporting

Use [Troubleshooting and Support](Troubleshooting.md) first. If the issue persists, select **Report a Bug** in the Main Menu or pause menu, or use the MSP603 bug-report form linked through Canvas/VLE. Include reproducible steps and your Unity/FMOD versions, but never include passwords, credentials, or unnecessary personal information.

## Academic integrity

Your submitted FMOD implementation, sound design and creative decisions must be your own work in accordance with the current assessment brief and university policy. AI output and technical examples can be incomplete; verify them and follow the university's current academic-integrity and AI-use policy.
