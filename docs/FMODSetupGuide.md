# Setting up FMOD after downloading the project

This guide assumes no previous Unity/FMOD integration experience. Complete Part A before beginning your assessed audio work in Part B.

## Required versions

- Unity Editor `6000.0.78f1`
- FMOD Studio `2.03.14`
- FMOD for Unity `2.03.14`

Use the approved **MSP603 v1.0.0 Student FMOD** release asset. Do not use GitHub's generated source archives or a lecturer/reference package.

## Part A — FMOD plugin / middleware setup

### 1. Install the applications

Install Unity `6000.0.78f1` through Unity Hub and install FMOD Studio `2.03.14` using the institution-approved installer or the official FMOD download route identified by your lecturer. Sign-in or licensing requirements belong to FMOD and your institution; do not share credentials in a support report.

The approved student project normally already contains FMOD for Unity `2.03.14`. After extraction, open the project in the required Unity version and allow the initial import, script compilation, and any FMOD asset refresh to finish before entering Play Mode. Import and compilation can take several minutes on first open.

### 2. Confirm the Unity integration

In Unity, confirm that:

- an **FMOD** menu is visible;
- **FMOD > Edit Settings** opens;
- official components such as **FMOD Studio Event Emitter** can be found in the Inspector;
- the Console contains no FMOD compilation errors.

Empty event selections and empty student project/bank paths are expected. Compilation errors, a missing FMOD menu, or missing FMOD component types indicate an integration problem.

### 3. Reintegrate only when necessary

If the FMOD menu or component types are missing after import, close Unity and follow the institution-approved FMOD for Unity `2.03.14` installation/reintegration method. Obtain that exact integration version through the approved FMOD or institutional route. Do not install a different version over the project and do not copy the lecturer FMOD project, banks, or completed event assignments.

Reopen Unity `6000.0.78f1`, allow the integration to import and all scripts to recompile, then repeat the confirmation checks above. If you are unsure whether reintegration is necessary, stop and use [Troubleshooting and Support](Troubleshooting.md) rather than deleting project content.

## Part B — student audio authoring

### 1. Understand the two projects

The downloaded Unity project contains the playable framework and FMOD integration. Your separate FMOD Studio project contains the audio events, parameters, routing, mixing, and banks that you author. Connecting them lets Unity discover and play your FMOD content; it does not copy lecturer content or create the assessed sound design for you.

Create your FMOD Studio project in a writable location you control. Keep its source audio and built banks together in a clear project structure, and back it up according to your module instructions.

### 2. Create and build FMOD content

1. Create the events and parameters required by your current assessment brief.
2. Reproduce standard names and labels exactly as listed in the [FMOD Authoring Reference](FMODAuthoringReference.md).
3. Assign every event you want Unity to discover to an appropriate bank.
4. Build the banks for your current desktop platform from FMOD Studio.

An event that is not assigned to a bank, or a bank that has not been built, will not be available correctly in Unity.

### 3. Connect Unity

1. In Unity, open **FMOD > Edit Settings**.
2. Configure Unity to use your FMOD Studio project or its built-bank directory, following the option demonstrated in your teaching session.
3. Use paths to your own project/banks, not paths from a lecturer machine or another student.
4. Build the banks in FMOD Studio and allow Unity's Event Browser/bank data to refresh.
5. Confirm that your event names appear in Unity.

If events remain missing, confirm the project/bank path, bank assignment, successful bank build, current platform, and Event Browser refresh before changing anything else.

### 4. Assign and test events

Use the supplied **Student FMOD Authoring** scene hierarchy and source prefabs under `Assets/MSP603/Resources/StudentAuthoring/`. Assign events through official FMOD components on those sources, not on temporary runtime clones.

The framework supplies playable states, authoring surfaces, parameters, and semantic hooks. You supply the event content, bank membership, assignments, parameter logic, spatialisation, routing, mixing, and creative intent. Intentionally blank events, emitters, and bank/project settings should remain blank until you author and assign your own work.

Test one simple event first, then bank loading, parameters, spatial behaviour, gameplay states, and mixing. Use the [FMOD Component Workflow](FMODComponentWorkflow.md) and [Vertical Slice Play Guide](VerticalSlicePlayGuide.md) for the next steps.

## Common setup failures

| Symptom | Check |
| --- | --- |
| FMOD menu is missing | Required integration version, completed Unity import, and Console compilation errors |
| Event is absent from Unity | Event bank assignment, successful bank build, configured project/bank path, and Event Browser refresh |
| Event appears but is silent | Audible content, correct event assignment, loaded bank, mixer routing, and volume/mute state |
| Banks cannot be found | Build platform and output location, then the path configured in **FMOD > Edit Settings** |
| References fail after moving computers | Reconnect to your own local FMOD project/banks and rebuild; do not preserve another machine's absolute path |
| Game plays but has little/no sound | Blank student audio assignments are intentional; confirm whether you have authored, built, and assigned the relevant events |

For recovery steps and reporting requirements, see [Troubleshooting and Support](Troubleshooting.md).

## YOU ARE READY WHEN…

- [ ] You are using Unity `6000.0.78f1`, FMOD Studio `2.03.14`, and FMOD for Unity `2.03.14`.
- [ ] The Unity project has finished importing and compiling without FMOD compilation errors.
- [ ] The FMOD menu and official FMOD components are available in Unity.
- [ ] You have created your own FMOD Studio project.
- [ ] Unity is connected to your project or built-bank directory.
- [ ] A test event is assigned to a bank, the bank builds successfully, and the event appears in Unity.
- [ ] You can assign that event to a supplied authoring source and hear it in Play Mode.
- [ ] You understand that blank framework assignments are intentional and that the assessed audio work is yours to author.
