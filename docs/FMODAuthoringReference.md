# MSP603 FMOD authoring reference

This is the student-safe catalogue of framework opportunities. It identifies what Unity publishes without revealing lecturer reference event assignments, source audio, banks, or completed sound-design choices.

## Required published parameters

| Parameter | Type | Values / use |
| --- | --- | --- |
| `RTPC_Master_Volume` | Global continuous | `0`–`100`; driven by Master volume controls. |
| `RTPC_Music_Volume` | Global continuous | `0`–`100`; driven by Music volume controls. |
| `RTPC_SFX_Volume` | Global continuous | `0`–`100`; driven by SFX volume controls. |
| `RTPC_WaveCounter` | Global labelled | `Wave0`, `Wave1`, `Wave2`, `Wave3`, `Wave4`, `Victory`, `Defeat`. |
| `Lives` | Global discrete | `0`–`20`; published at level start and on life loss. |
| `TowerType` | Local labelled | `Cannon`, `Archer`, `Inferno`, `Mage` on tower authoring Prefabs. |
| `UpgradeLevel` | Local labelled | `1`, `2`, `3` for tower state work. |
| `Activity` | Local labelled | `Idle`, `Firing`, `Targeting` for tower state work. |

## Authoring surfaces

- **Student FMOD Authoring** scene objects expose music, ambience, UI, settings, wave, outcome, and gameplay opportunities.
- Tower, enemy, and projectile Prefabs under `Resources/StudentAuthoring` are the persistent sources for dynamically created gameplay objects.
- The FMOD runtime bridge publishes shared parameters and optional semantic mappings. It complements, rather than replaces, official Inspector-led FMOD components.

## Game and wave opportunities

- Level music and level ambience can run from the pre-wave `Wave0` state.
- `WaveStarted` and `WaveEnded` provide one-shot transition opportunities.
- `RTPC_WaveCounter` publishes `Wave0`, `Wave1`, `Wave2`, `Wave3`, `Wave4`, `Victory`, and `Defeat` for adaptive music, ambience, mixing, or other state-driven design.
- Victory and defeat are terminal states, not extra waves. Test how persistent material enters and leaves those states.

## Player opportunities

- `Lives` is published at level start and whenever a life is lost.
- `PlayerLifeLost` is available for immediate feedback when an enemy reaches the keep.

## Tower lifecycle and identity

All four tower families—Cannon, Archer, Mage, and Inferno—have source prefabs and support placement, targeting/activity, three upgrade tiers, selling, and destruction.

- `TowerBuilt`, `TowerUpgraded`, `TowerUpgradeFailed`, `TowerSold`, and `TowerDestroyed` describe shared lifecycle moments.
- Each tower instance can publish local `TowerType`, `UpgradeLevel`, and `Activity` values through the supplied tower-parameter component.
- Tower prefabs expose placed, attack, upgrade, and destroyed UnityEvents for official-component workflows.

These semantic identifiers are gameplay hooks, not prescribed FMOD event paths. Use only mappings and assignments appropriate to your brief.

## Tower attacks and projectile impacts

- Archer, Mage, and Cannon expose distinct firing opportunities: `ArcherFired`, `MageFired`, and `CannonFired`.
- Their projectile source prefabs expose an impact UnityEvent, and `ProjectileImpact` is available as a semantic opportunity.
- Compare attack rate, distance, impact character, spatial position, and upgrade/activity state rather than treating all projectile towers identically.

## Inferno sustained behaviour

Inferno does not use the same projectile lifecycle as Archer, Mage, and Cannon. Its sustained flame provides:

- `InfernoPlaced`;
- `InfernoFlameStarted`;
- repeated `InfernoBurn` opportunities while damage is sustained;
- `InfernoFlameStopped`;
- `InfernoDestroyed`.

The Inferno flame prefab also exposes start, burn, and stop UnityEvents. Design start, loop/sustain, variation, and release behaviour carefully so repeated burn ticks do not produce unintended clutter.

## Enemy lifecycle and movement

The canonical enemy types are **Goblin Raider**, **Goblin Brute**, and **Ruin Knight**. Their source prefabs provide distinct locations for type-specific authoring.

- `EnemySpawned`, `EnemyHit`, `EnemyDefeated`, and `EnemyEscaped` cover the main lifecycle.
- Enemy prefabs expose movement-started, damaged, and died UnityEvents.
- The framework publishes enemy movement start/stop state for persistent movement or foley work.

No approved global enemy-type parameter is supplied; use the separate enemy source prefabs where type-specific behaviour is required.

## Music, ambience, outcomes, and interface

Scene-level **Student FMOD Authoring** objects expose sources for level music, ambience, wave transitions, life loss, victory/defeat, tower actions, and interface controls. The Main Menu exposes music, ambience, play, level selection, settings, quit, back, and bug-report interactions. Gameplay exposes pause, start-wave, upgrade, sell, navigation, and settings controls.

The Master, Music, and SFX sliders publish the three continuous global parameters listed above. Music Enabled and SFX Enabled switches publish framework mix state that may be mapped to appropriate buses or VCAs; they are not additional approved student parameter-name contracts. Confirm control behaviour in both the Main Menu and pause/settings interface.

## Suggested learning sequence

1. Make one assigned event audible and load its bank.
2. Add spatial behaviour to a scene object or source prefab.
3. Test a one-shot gameplay opportunity.
4. Use a local tower parameter on independent tower instances.
5. Use a global wave or lives parameter.
6. Add music/ambience and terminal-state behaviour.
7. Implement settings/mixer control and test the full vertical slice.

## Important boundary

The framework publishes state; you author the audible behaviour. Follow [FMOD Component Workflow](FMODComponentWorkflow.md), use the exact approved parameter names above, and test against the [Vertical Slice Play Guide](VerticalSlicePlayGuide.md).
