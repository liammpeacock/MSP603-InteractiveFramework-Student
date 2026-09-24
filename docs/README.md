# MSP603 Student Documentation

This is the documentation index for the **MSP603 Interactive Framework v1.1.0**.

The Student distribution provides the playable Unity framework, the supplied Student FMOD Studio project, required integration structures and known-good compiled reference banks.

It does **not** provide the Lecturer source audio or completed Lecturer sound-design implementation.

These documents explain how to use the framework while keeping the creative and technical audio implementation as your own work.

---

## Start here

Begin with the [Student Guide](StudentGuide.md).

If this is your first time using the framework, follow the documentation approximately in this order:

1. [Student Guide](StudentGuide.md)
2. [Getting Started](GettingStarted.md)
3. [Required Software](RequiredSoftware.md)
4. [FMOD Setup Guide](FMODSetupGuide.md)
5. [Vertical Slice Play Guide](VerticalSlicePlayGuide.md)
6. [Project Navigation](ProjectNavigation.md)
7. [FMOD Authoring Reference](FMODAuthoringReference.md)
8. [FMOD Component Workflow](FMODComponentWorkflow.md)
9. [Assessment Authoring Workflow](AssessmentAuthoring.md)
10. [Troubleshooting and Support](Troubleshooting.md)

You do not need to read every document from beginning to end before starting work. Return to the relevant guide as your implementation develops.

---

## Documentation index

| VLE topic | Resource | Use it when |
| --- | --- | --- |
| Student guide | [Student Guide](StudentGuide.md) | You need to understand the framework, what is supplied and what you author. |
| Getting started | [Getting Started](GettingStarted.md) | You have downloaded the approved project and want to test the known-good starting point. |
| Required software | [Required Software](RequiredSoftware.md) | You need to install or check the required Unity and FMOD versions. |
| Unity setup | [Getting Started](GettingStarted.md#open-the-unity-project) | You are opening the Unity project for the first time. |
| FMOD setup | [FMOD Setup Guide](FMODSetupGuide.md) | You are opening the supplied Student FMOD project, checking the integration, building banks or recovering the FMOD for Unity plugin. |
| Play guide | [Vertical Slice Play Guide](VerticalSlicePlayGuide.md) | You need to learn the game or systematically exercise audio opportunities and game states. |
| Project navigation | [Project Navigation](ProjectNavigation.md) | You need to locate scenes, Student authoring objects, source Prefabs, the supplied FMOD project or runtime hooks. |
| Authoring opportunities | [FMOD Authoring Reference](FMODAuthoringReference.md) | You need the supported events, parameters, mixer architecture and gameplay-state opportunities. |
| FMOD component workflow | [FMOD Component Workflow](FMODComponentWorkflow.md) | You are working with the supplied authoring surfaces and official FMOD Unity components. |
| Assessment workflow | [Assessment Authoring Workflow](AssessmentAuthoring.md) | You are planning, implementing, testing or documenting your own creative response. |
| Recovery and support | [Troubleshooting and Support](Troubleshooting.md) | Something is not behaving as expected in Unity or FMOD. |

---

## Learn, apply, test

The framework is designed around a simple learning process:

**Learn it from scratch → understand it → recognise it in the assignment framework → apply it creatively → test it in the game.**

During teaching, you may use separate clean FMOD projects to learn techniques such as:

- creating events;
- creating parameters;
- mixer construction;
- routing;
- effects;
- spatialisation;
- bank building.

For the assessment framework itself, use the supplied Student FMOD project rather than replacing it with a separate project.

---

## Reference implementation

The Student Unity project initially includes compiled reference banks containing known-good reference audio.

Use these to establish that the framework works before making substantial changes.

They provide a **technical and diagnostic reference**, not source material for your assessment.

As you develop your own FMOD implementation and build your own banks, your implementation replaces the relevant reference behaviour.

---

## Living documentation and release snapshots

These GitHub pages are the **living documentation** for the framework.

Documentation included inside a downloaded release package is the frozen snapshot supplied with that particular release.

Canvas/VLE can therefore link to:

- these pages for current guidance; and
- the approved release asset for the project download.

If guidance has been clarified after a release was packaged, use the current GitHub documentation alongside the project version specified by your teaching team.

---

## Use the approved project

Use the approved Student project supplied for the module.

Do not use:

- a Lecturer-controlled repository;
- a Lecturer reference project;
- a generic GitHub source archive;
- a separately created replacement FMOD project

as your assessment project.

The supplied Student package contains the matching Unity and FMOD structures required for the framework.

If you experience a problem, use [Troubleshooting and Support](Troubleshooting.md) before deleting, replacing or rebuilding framework structures.