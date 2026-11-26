# FMOD setup — Student v2 candidate

v2.0.0 is in preparation and is not yet an approved download. This guide describes the intended audio-free package. Existing v1.1.0 users should follow that release's documentation and software requirements.

Use Unity 6000.0.78f1, FMOD Studio 2.03.13 and the included FMOD for Unity 2.03.13. Do not import a second integration or silently upgrade it.

## Open the supplied projects

1. Download the versioned release ZIP when approved, verify its published SHA-256, and extract into a new local writable folder. Keep adjacent Unity and Audio directories together. Never overwrite an existing assessment project.
2. Open `Unity/MSP603_26-27` through Unity Hub. Allow package resolution/import to finish, then open `Assets/MSP603/Scenes/MainMenu.unity`.
3. Open the supplied `Audio/FMOD/Student/Student.fspro` in Studio. Preserve event identities/paths, parameter definitions, mixer structure and bank membership. Do not create an unrelated replacement project.
4. The release is intended to include six freshly built, matching **audio-free** Desktop banks. Unity gameplay and parameter/emitter plumbing work, but music, footsteps, effects and pause ambience are deliberately silent until you author content. Silence alone is not an integration fault.
5. Add your own appropriately licensed source audio to the empty authoring instruments, develop your creative parameter/mixing response, and build Desktop banks. Preserve the configured relative source project/bank paths and loader lists; let Unity refresh those banks.
6. Test MainMenu and all three levels, settings/volume controls, pause/resume, parameter resets and Lives=20 initialization. Observe globals via Live Update while using the existing Run In Background setting.

## Required behavior

EnemyCount, RunnerCount, GruntCount and ArmouredCount are Global Continuous/game-controlled, 0–60/default 0. Paused is Global Labelled with Paused/Unpaused labels and initial Unpaused. Preserve existing Lives, wave, volume and local tower parameters including Proximity. Follow [the authoring reference](FMODAuthoringReference.md).

The mixer separates Gameplay/Music, Gameplay/SFX (Towers/Enemies), Gameplay/Ambience and UI. Gameplay gain is attenuated when paused; UI is outside that attenuation. Empty events still retain their assignment/routing surfaces. The absence of audio samples is intentional; missing parameters, missing bank files, unresolved event references or Console errors are not.

Do not copy Lecturer/Canonical source audio or their compiled banks into this assessment project. Old Student reference banks and private Student-Dev banks contain authored audio and are not substitutes for v2's audio-free banks.

Lives is published before LifeLost starts so conditional instruments receive the new value. Sequential Global Scope playlist history is separate and is not reset by this ordering fix. Build a parameter-conditioned countdown if deterministic selection is required.
