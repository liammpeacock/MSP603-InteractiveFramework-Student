# Project navigation

| Need | Location |
| --- | --- |
| Start menu and level selection | `Assets/MSP603/Scenes/MainMenu.unity` |
| Playable levels | `Assets/MSP603/Scenes/Level01.unity`, `Level02.unity`, `Level03.unity` |
| Scene-level authoring sources | Expand **Student FMOD Authoring** in each scene |
| Tower authoring Prefabs | `Assets/MSP603/Resources/StudentAuthoring/Towers/` |
| Enemy authoring Prefabs | `Assets/MSP603/Resources/StudentAuthoring/Enemies/` |
| Projectile authoring Prefabs | `Assets/MSP603/Resources/StudentAuthoring/Projectiles/` |
| Runtime FMOD state bridge | `Assets/MSP603/FMOD/Runtime/` |

The tower sources are Cannon, Archer, Mage, and Inferno. Enemy sources are Goblin Raider, Goblin Brute, and Ruin Knight; projectile sources include Archer, Mage, and Cannon projectiles plus the Inferno flame.

Configure source scene objects and Prefabs, not temporary runtime clones. Standard FMOD components added to these sources are inherited by running gameplay objects where the framework uses them. Use the supplied authoring surfaces and avoid unnecessary changes to gameplay scripts, scenes, project settings, or framework state hooks.

See the [FMOD Authoring Reference](FMODAuthoringReference.md) for the opportunities exposed at these locations.
