# Tower-defence vertical slice play guide

Use this guide to learn the game and deliberately exercise states that can drive your audio work.

## Play the reference game first

**[Play MSP603 Tower Defence in your browser](https://bimmmcrliampeacock.itch.io/msp603-tower-defence)** — no installation is required.

The browser version is the quickest way to learn the finished gameplay, explore all three levels, and identify states you may want to respond to with audio. It is a reference/play-testing route, not your assessment workspace: create and test your own audio implementation in the approved Unity/FMOD student project.

Reference-game controls include mouse/primary click for menus and gameplay, **Escape** for pause/back, the in-game speed control for 1×/2× play, and teaching/demo shortcuts **F8** (add 500 gold), **F9** (skip to Victory), and **F10** (skip to Defeat).

## Open your student project

1. Open the approved student project in Unity `6000.0.78f1`.
2. Open `Assets/MSP603/Scenes/MainMenu.unity`.
3. Enter Play Mode and select **Play Level 1**.

The framework contains three playable scenes: `Level01`, `Level02`, and `Level03`. Each level has four waves. You start a run with **20 lives** and lose lives when enemies reach the keep.

## Build and manage towers

1. Click a visible build pad.
2. Choose **Cannon**, **Archer**, **Mage**, or **Inferno** from the build panel.
3. Select an occupied pad to see its tower type, tier, upgrade cost, **Upgrade**, and **Sell** actions.
4. Upgrade towers and confirm their behaviour at tiers 1, 2, and 3.
5. Sell a tower to receive 60% of the gold invested and make the pad available again.
6. Select **Start Wave 1** when ready, earn gold by defeating enemies, and prepare between waves.

## Compare all four towers

| Tower | Gameplay behaviour | Useful audio tests |
| --- | --- | --- |
| Archer | Affordable, fast single-target projectile attacks | Fast attack rhythm, projectile travel/impact, targeting, upgrade variation |
| Mage | Slower, high-damage single-target projectiles | Contrasting magical attack identity, projectile impact, upgrades |
| Cannon | Slow area-damage projectiles against groups | Heavy firing, travel and splash-impact character, group combat |
| Inferno | Sustained flame against a target | Placement, flame start, sustained burn, stop/release, targeting and destruction |

Place every tower at least once. Compare attack pace, targeting, range, and the difference between the three projectile towers and Inferno's sustained behaviour. Upgrade and sell representative towers so you can test lifecycle and parameter changes, not only firing sounds.

## Play all three levels

Complete `Level01`, then use level selection or progression to test `Level02` and `Level03`. Observe differences in route, build-pad placement, enemy pressure, starting resources, and combat density. Confirm the pre-wave state, all four waves, victory, defeat, restart, and level transitions where relevant to your work.

The canonical enemies are **Goblin Raider**, **Goblin Brute**, and **Ruin Knight**. Compare movement, hit, defeat, spawn, and escape opportunities across them.

## Audio test pass

Exercise the states you intend to author:

- pre-wave, wave start/end, `Wave1`–`Wave4`, victory, and defeat;
- level music and ambience;
- tower placement, targeting, firing, upgrading, failed upgrading, selling, and destruction;
- Archer, Mage, and Cannon projectile impacts;
- Inferno flame start, sustained burn, stop, placement, and destruction;
- enemy spawn, movement, hit, defeat, and escape;
- player-life loss;
- menu, level selection, gameplay controls, and settings;
- Master, Music, and SFX sliders, plus Music/SFX enabled switches and associated mixer/VCA behaviour.

Test multiple placed towers to confirm spatial separation and instance-specific `TowerType`, `UpgradeLevel`, and `Activity` behaviour. Rebuild banks and retest after material FMOD changes.

## Pause and audio controls

Select **Pause** in the upper-right corner during gameplay, or press **Escape**, to pause or resume. The pause/settings interfaces provide Master, Music, and SFX volume controls plus Music Enabled and SFX Enabled switches. Your FMOD implementation determines how the supplied control state affects authored events, routing, and VCAs.

If behaviour differs from this guide, use [Troubleshooting and Support](Troubleshooting.md) and record the scene, wave, tower/enemy, expected result, actual result, and reproduction steps.
