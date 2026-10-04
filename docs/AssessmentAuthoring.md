# Assessment Authoring Workflow

Use this workflow when applying the MSP603 Interactive Framework to your current assessment work.

The framework provides the playable game, integration structures and gameplay information. Your assessed creative response is developed through the audio implementation you create using those opportunities.

Always use the current assessment brief and teaching guidance as the authoritative source for assessment requirements.

---

## 1. Identify the opportunity

Identify the game event, state or behaviour you want to address.

This might involve:

- a one-shot gameplay event;
- a persistent sound;
- music;
- ambience;
- an interface interaction;
- a global game state;
- a local object state;
- spatial behaviour;
- parameter-driven behaviour.

Use [Project Navigation](ProjectNavigation.md) and the [FMOD Authoring Reference](FMODAuthoringReference.md) to identify the relevant framework opportunity.

---

## 2. Identify what the framework already provides

Before creating anything new, check whether the Student framework already provides:

- an FMOD event;
- a parameter;
- a Unity Event Emitter;
- a source Prefab;
- a scene authoring object;
- a bank structure;
- a gameplay-state hook.

Some of these structures are deliberately supplied because Unity expects a stable integration contract.

Do not recreate a supplied framework structure simply to demonstrate that you know how to create it.

You can demonstrate your understanding through the way you use and develop it.

---

## 3. Define your creative intention

Before implementing the sound, decide what you want it to achieve.

Consider questions such as:

- What information should the player receive?
- What should the sound communicate?
- Should it change over time?
- Should it respond to gameplay state?
- Does it need variation?
- Should it be spatial?
- How should it relate to music, ambience and other sound effects?
- Where should it sit in the mix?

Your creative intention should guide the technical implementation rather than the available technology deciding the design for you.

---

## 4. Author the implementation

Use the supplied Student FMOD project for the assessment framework.

Depending on the opportunity, your work might include:

- importing or creating your own audio;
- editing and layering material;
- designing event behaviour;
- applying parameter behaviour;
- creating variation;
- routing through the mixer;
- using effects and processing;
- configuring spatialisation;
- designing adaptive behaviour;
- balancing the mix.

The supplied event or parameter is the connection point.

**What you make it do is your implementation.**

---

## 5. Build and test

Save the FMOD project and build the relevant banks.

Return to Unity and test the implementation in the actual gameplay context.

Do not rely only on previewing the event inside FMOD Studio.

Where appropriate, test:

- normal behaviour;
- repeated behaviour;
- different gameplay states;
- different tower or enemy instances;
- parameter boundaries;
- pause/resume;
- Victory and Defeat;
- settings controls;
- transitions between states.

Use FMOD Live Update where it helps you understand what the game is publishing while you test.

---

## 6. Diagnose before rebuilding

If the implementation does not behave as expected, identify where the problem occurs.

The supplied reference banks provide a useful technical baseline.

If the reference implementation worked before you built your own banks, investigate your FMOD authoring, bank build, routing and parameter behaviour before changing the Unity framework.

Use [Troubleshooting and Support](Troubleshooting.md) before deleting or recreating supplied integration structures.

---

## 7. Refine

Interactive audio normally requires iteration.

Listen to your implementation in context and refine areas such as:

- timing;
- variation;
- repetition;
- balance;
- clarity;
- parameter response;
- spatial behaviour;
- transitions;
- interaction with other sounds.

Test changes in the game rather than making decisions entirely from isolated FMOD playback.

---

## 8. Record your decisions

Keep track of what you changed and why.

Your documentation and critical work should distinguish between:

### Supplied framework

Examples include:

- gameplay systems;
- supplied event identities;
- supplied parameter identities;
- bank structure;
- Unity integration;
- authoring hooks.

### Your implementation

Examples can include:

- your audio material;
- event design;
- parameter application;
- routing;
- processing;
- spatial decisions;
- adaptive behaviour;
- mixing;
- testing and refinement;
- creative rationale.

A supplied framework structure should not be presented as something you created yourself.

Your contribution is demonstrated through how you use the framework to produce and justify your own audio implementation.

---

## Important boundary

The framework publishes state and provides authoring opportunities.

It must not decide the assessed sound design for you.

The presence of a supplied event, parameter, bank or component does not determine:

- what the game should sound like;
- what audio you should use;
- how a parameter should affect the sound;
- how events should be routed;
- how the mix should behave;
- how spatialisation should be designed;
- what creative decisions you should make.

Use the framework as the technical foundation for your own work.

For implementation guidance, continue with the [FMOD Component Workflow](FMODComponentWorkflow.md) and [FMOD Authoring Reference](FMODAuthoringReference.md).