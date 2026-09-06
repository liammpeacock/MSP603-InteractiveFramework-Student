# Tower-Defence Vertical Slice Play Guide

## Open the Game

1. Open the approved student project in Unity `6000.0.78f1`.
2. Open `Assets/MSP603/Scenes/MainMenu.unity`.
3. Enter Play Mode and select **Play Level 1**.

## Play Level 1

1. Click a visible production build pad and choose Archer, Mage, or Cannon from its build panel.
2. Click an occupied pad to view its tower type, tier, upgrade cost, Upgrade, and Sell actions.
3. Towers have three tiers. Selling refunds 60% of the gold invested and makes the pad available again.
4. Select **Start Wave 1** when ready.
5. Earn gold by defeating enemies and prepare for the next wave.
6. Prevent enemies from reaching the keep. The level ends after four cleared waves or when all fifteen lives are lost.

## Pause and Audio Controls

Select **Pause** in the upper-right corner during Level 1, or press **Escape**, to pause or resume. The pause menu provides Master, Music, and SFX volume sliders plus Music Enabled and SFX Enabled switches. In the middleware-free project these publish testable audio-control events; in the FMOD project the configured bridge maps them to VCAs.

## Tower Roles

| Tower | Role |
| --- | --- |
| Archer | Affordable, fast single-target damage |
| Mage | High damage against priority targets |
| Cannon | Slow area damage against groups |

## Current Status

This is a grey-box prototype. Primitive shapes, interface styling, combat balance, and silent audio hooks are expected at this stage. Record bugs with the scene, wave, tower, expected result, actual result, and reproduction steps.
