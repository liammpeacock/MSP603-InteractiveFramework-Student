# FMOD Component Workflow

Use the official FMOD Studio Unity components for visible, Inspector-led authoring.

The MSP603 runtime bridge publishes gameplay state and provides additional integration where required. It does not replace normal FMOD event, parameter, bank, spatialisation, routing or mixing work.

The supplied Student FMOD project provides the integration scaffold for the assessment project. You develop the audible implementation within that framework.

---

## Two different learning workflows

During MSP603 you may work with FMOD in two different ways.

### 1. Learning a technique from scratch

Your lecturer may use a clean or empty FMOD project when introducing a technique.

For example, you might learn how to:

- create an event;
- create a parameter;
- configure parameter behaviour;
- create mixer buses;
- route events;
- use effects;
- configure spatialisation;
- build banks.

This is useful because you experience the complete process rather than only editing something that already exists.

### 2. Applying the technique to the assignment framework

The MSP603 assignment project already contains structures that Unity expects, including supplied events, parameters and banks.

When working on the assignment project, do not recreate these structures simply because you created them from scratch during a teaching exercise.

Instead:

**Learn it from scratch → understand it → find the corresponding structure in the assignment project → apply the technique.**

For example, you might create a local continuous parameter yourself during a classroom exercise before using the supplied local `Proximity` parameter creatively in the assignment framework.

---

## Working with Unity components

### Scene-based sources

In a scene, expand:

**Student FMOD Authoring**

Select the source object appropriate to the behaviour you are implementing.

These objects expose authoring opportunities for areas such as:

- music;
- ambience;
- interface audio;
- settings;
- wave transitions;
- life loss;
- pause;
- Victory and Defeat;
- tower interactions.

### Dynamically created gameplay objects

For spawned systems, work with the corresponding source Prefab under:

`Assets/MSP603/Resources/StudentAuthoring/`

This includes sources associated with:

- Cannon;
- Archer;
- Mage;
- Inferno;
- Goblin Raider;
- Goblin Brute;
- Ruin Knight;
- projectiles;
- Inferno flame behaviour.

Configure the source Prefab rather than a temporary runtime clone.

Changes made only to an object created during Play Mode will not provide a reliable authoring workflow.

---

## Official FMOD components

Depending on the task, you may encounter or configure official FMOD components such as:

- **FMOD Studio Event Emitter**
- **Studio Parameter Trigger**
- **Studio Global Parameter Trigger**
- **Studio Bank Loader**

Use the appropriate component for the behaviour you are trying to implement.

The runtime bridge complements these components. It should not be treated as a replacement for understanding normal FMOD/Unity integration.

---

## Event workflow

For a typical event:

1. Identify the relevant gameplay opportunity.
2. Locate the corresponding source object or Prefab in Unity.
3. Identify the supplied event in the Student FMOD project.
4. Add or author your own audio content.
5. Design the event behaviour.
6. Route the event appropriately through the mixer.
7. Check its bank assignment.
8. Build the relevant bank.
9. Return to Unity.
10. Trigger the gameplay action.
11. Listen, diagnose and refine.

Work incrementally.

A single functioning event is more useful during development than changing many events before testing any of them.

---

## Parameter workflow

The framework publishes the approved parameters documented in the [FMOD Authoring Reference](FMODAuthoringReference.md).

When using the supplied assignment project:

1. identify the gameplay behaviour you want to respond to;
2. find the corresponding supplied parameter;
3. understand whether it is Global or Local;
4. confirm its values, labels or range;
5. decide what it should control in your FMOD implementation;
6. author the response;
7. test it while the game is running.

Do not rename supplied framework parameters.

Unity and FMOD need to agree on the parameter contract.

---

## Global parameters

Global parameters describe information available across the wider game or mix.

Examples include:

- `RTPC_Master_Volume`
- `RTPC_Music_Volume`
- `RTPC_SFX_Volume`
- `RTPC_WaveCounter`
- `Lives`
- `Paused`

For example, `RTPC_WaveCounter` reports the current wave or terminal state.

It does not automatically decide what your music should do.

Similarly, `Paused` reports whether gameplay is paused.

It does not prescribe a filter, snapshot, volume change or other sonic treatment.

The framework publishes the state.

**You author the response.**

---

## Local parameters

Local parameters belong to individual event instances.

Tower authoring includes:

- `TowerType`
- `UpgradeLevel`
- `Activity`
- `Proximity`

This allows different tower instances to report different values simultaneously.

For example, moving the cursor near one tower can give that tower a high `Proximity` value while another tower remains low.

When designing with local parameters, test multiple instances rather than assuming that behaviour observed on one tower represents the entire system.

---

## Spatial behaviour

Spatialisation and parameter-driven proximity are related teaching opportunities, but they are not the same system.

### Genuine FMOD 3D spatialisation

Use FMOD's spatial tools when an event should behave as a positioned source in relation to the game's listener.

Investigate appropriate:

- emitter position;
- listener position;
- attenuation;
- spatializer behaviour;
- minimum and maximum distance;
- other spatial properties.

### Parameter-driven `Proximity`

`Proximity` is gameplay information published independently for each tower.

It describes the relationship between the cursor and that tower.

It does not create another FMOD listener.

You can use `Proximity` to control audible behaviour independently of, or alongside, genuine 3D spatialisation.

When testing, be able to explain which behaviour comes from FMOD spatialisation and which comes from parameter automation.

---

## Mixer routing

The Student project deliberately does not provide the completed Lecturer mixer routing.

Routing is part of your implementation work.

Consider the intended high-level distinction between:

- gameplay;
- ambience;
- music;
- sound effects;
- enemy sounds;
- tower sounds;
- interface audio.

The framework's Music and SFX controls depend on sensible routing and control behaviour.

If an event plays correctly in FMOD but behaves unexpectedly with the game's settings controls, investigate its mixer routing.

UI audio is deliberately distinguishable from gameplay SFX.

Do not assume every audible event belongs under the same SFX control.

---

## Banks

Bank building is part of the authoring loop.

After making relevant changes:

1. save your FMOD project;
2. build the appropriate banks;
3. return to Unity;
4. allow FMOD/Unity to refresh;
5. test the affected gameplay behaviour.

The compiled reference banks supplied with the Student project establish a known-good starting point.

As you build banks from your own FMOD implementation, your authored behaviour replaces the relevant reference behaviour.

If something worked before you rebuilt your banks but fails afterwards, that distinction can help you identify whether the problem is in the Unity framework or your FMOD authoring.

---

## FMOD Live Update

Unity is configured to continue running when it loses application focus.

This supports FMOD Live Update workflows during teaching and development.

Live Update can help you observe and investigate:

- event instances;
- global parameters;
- local parameters;
- `Paused`;
- `Proximity`;
- mixer behaviour;
- adaptive systems;
- spatial behaviour.

Use Live Update as a diagnostic and authoring tool rather than as a substitute for saving your work and building the required banks.

---

## Recommended learning order

1. Play a single event using an official FMOD Studio Event Emitter.
2. Understand how that event reaches Unity through its bank.
3. Route the event through the mixer.
4. Change a local event parameter.
5. Change a global parameter.
6. Test genuine listener/emitter spatial behaviour.
7. Compare genuine 3D behaviour with parameter-driven `Proximity`.
8. Develop adaptive wave, pause or lives behaviour.
9. Configure mixer/settings control.
10. Test the complete vertical slice.

You may learn several of these techniques from scratch in separate teaching exercises before applying them to the supplied assignment framework.

---

## The key principle

The Unity framework answers questions such as:

- What happened?
- Which object did it happen to?
- What state is the game in?
- What value should FMOD receive?

Your FMOD implementation answers:

- What should the player hear?
- How should it vary?
- How should it respond?
- Where should it sit in the mix?
- How should it behave spatially?
- How does it support the game?

That distinction is central to working with the MSP603 framework.

For first-time setup and recovery instructions, see the [FMOD Setup Guide](FMODSetupGuide.md).

For the complete list of available framework parameters and opportunities, see the [FMOD Authoring Reference](FMODAuthoringReference.md).

For problems during implementation, see [Troubleshooting and Support](Troubleshooting.md).