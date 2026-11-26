# FMOD component workflow

Use the official FMOD Studio Unity components for visible, Inspector-led authoring. The runtime bridge is an advanced state publisher; it does not replace normal FMOD event, parameter, bank, spatialisation, or mixing work.

## Native components

1. In a scene, expand **Student FMOD Authoring** and select a meaningful source object.
2. On spawned systems, select the corresponding source Prefab under `Assets/MSP603/Resources/StudentAuthoring/`.
3. Add or configure an official **FMOD Studio Event Emitter**, **Studio Parameter Trigger**, **Studio Global Parameter Trigger**, or bank loader as appropriate.
4. Assign events from your own connected FMOD project, build banks, and test.

Configure source objects or Prefabs rather than transient runtime clones.

## Adaptive state and controls

Create the parameters listed in [FMOD Authoring Reference](FMODAuthoringReference.md) exactly. The three volume sliders publish continuous global values. `RTPC_WaveCounter` reports level load, each wave, and the terminal `Victory`/`Defeat` state; it does not secretly start or stop a music event.

## Learning order

1. Play a single event with a Studio Event Emitter.
2. Load its bank with Studio Bank Loader.
3. Change a local event parameter.
4. Change a global parameter.
5. Test listener and spatial behaviour.
6. Configure VCAs and settings controls.
7. Map a procedural gameplay hook only when needed.

Apply the workflow to scene-level music/ambience and interface sources, the Cannon/Archer/Mage/Inferno tower prefabs, Goblin Raider/Goblin Brute/Ruin Knight enemy prefabs, and projectile/Inferno-flame sources as appropriate to your brief.

For first-time connection and recovery instructions, see [Setting Up FMOD After Downloading the Project](FMODSetupGuide.md) and [Troubleshooting and Support](Troubleshooting.md).
