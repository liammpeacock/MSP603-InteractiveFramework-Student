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

## Important boundary

The framework publishes state; you author the audible behaviour. Follow [FMOD Component Workflow](FMODComponentWorkflow.md), use the exact names above, and test against the [Vertical Slice Play Guide](VerticalSlicePlayGuide.md).
