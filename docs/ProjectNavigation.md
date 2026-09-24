# Project Navigation

Use this page to find the main authoring locations in the MSP603 Student project.

## Unity project

| Need | Location |
| --- | --- |
| Start menu and level selection | `Assets/MSP603/Scenes/MainMenu.unity` |
| Playable levels | `Assets/MSP603/Scenes/Level01.unity`, `Level02.unity`, `Level03.unity` |
| Scene-level FMOD authoring sources | Expand **Student FMOD Authoring** in each scene |
| Tower authoring Prefabs | `Assets/MSP603/Resources/StudentAuthoring/Towers/` |
| Enemy authoring Prefabs | `Assets/MSP603/Resources/StudentAuthoring/Enemies/` |
| Projectile authoring Prefabs | `Assets/MSP603/Resources/StudentAuthoring/Projectiles/` |
| Runtime FMOD state bridge | `Assets/MSP603/FMOD/Runtime/` |

---

## FMOD Studio project

The supplied Student FMOD project is:

`Audio/FMOD/MSP603_26-27_TD/MSP603_26-27_TD.fspro`

Use this project when authoring audio for the assessment framework.

It contains the event, parameter and bank identities expected by the Unity integration.

Do not create a replacement FMOD project for the assignment framework.

The Student FMOD project is deliberately a scaffold rather than the completed Lecturer implementation. Your own audio, event behaviour, routing, mixing, parameter application and other creative implementation are developed here.

---

## Tower sources

The four tower sources are:

- Cannon
- Archer
- Mage
- Inferno

Tower authoring Prefabs provide opportunities associated with areas such as:

- placement;
- attacks;
- upgrades;
- failed upgrades;
- selling;
- destruction;
- `TowerType`;
- `UpgradeLevel`;
- `Activity`;
- `Proximity`.

Because towers are created dynamically during gameplay, make persistent authoring changes to the appropriate source Prefab rather than a temporary tower instance created during Play Mode.

---

## Enemy sources

The three canonical enemy sources are:

- Goblin Raider
- Goblin Brute
- Ruin Knight

Enemy authoring Prefabs provide opportunities associated with movement, damage, defeat and other lifecycle behaviour.

Use the separate source Prefabs when different enemy types require different implementations.

---

## Projectile and Inferno sources

Projectile authoring sources include:

- Archer projectile;
- Mage projectile;
- Cannon projectile;
- Inferno flame.

Remember that Inferno uses sustained flame behaviour rather than the same projectile lifecycle as Archer, Mage and Cannon.

See the [FMOD Authoring Reference](FMODAuthoringReference.md) for the available opportunities.

---

## Scene-level authoring

Each relevant scene contains a **Student FMOD Authoring** hierarchy.

Use these objects for scene-level opportunities such as:

- music;
- ambience;
- interface audio;
- settings;
- wave transitions;
- player-life loss;
- pause;
- Victory and Defeat.

These objects provide stable, visible authoring locations rather than requiring you to search through unrelated gameplay objects.

---

## Source Prefab or runtime object?

As a general rule:

**If the game creates the object during Play Mode, make persistent authoring changes to its source Prefab.**

Do not configure a temporary runtime clone and expect that change to remain after Play Mode ends.

Standard FMOD components configured on the appropriate source Prefabs can then be inherited by the gameplay objects created from them.

---

## Runtime FMOD bridge

The runtime FMOD integration is located under:

`Assets/MSP603/FMOD/Runtime/`

This code helps publish gameplay information to FMOD.

For normal Student authoring, you should not need to edit these scripts.

Use the supplied:

- scene authoring objects;
- source Prefabs;
- official FMOD components;
- supplied parameters;
- supplied Student FMOD project.

The runtime bridge provides the connection between gameplay state and FMOD; your work is primarily to determine what that information should sound like.

---

## Avoid unnecessary framework changes

Unless specifically required by your current brief or agreed with your lecturer, avoid unnecessary changes to:

- gameplay scripts;
- framework FMOD scripts;
- scenes;
- project settings;
- supplied event identities;
- supplied parameter identities;
- framework state hooks.

This makes your audio implementation easier to test, troubleshoot and distinguish from the supplied game framework.

---

## Where next?

For the available events, parameters and gameplay opportunities, use the [FMOD Authoring Reference](FMODAuthoringReference.md).

For practical implementation, use the [FMOD Component Workflow](FMODComponentWorkflow.md).

For first-time setup, use the [FMOD Setup Guide](FMODSetupGuide.md).

If something does not behave as expected, use [Troubleshooting and Support](Troubleshooting.md).