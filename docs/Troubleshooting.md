# Troubleshooting and support

## FMOD transfer or reinstall recovery

FMOD-for-Unity integrations can occasionally become invalid after a project is downloaded, copied, zipped, unzipped, or transferred.

1. Confirm you are using Unity `6000.0.78f1`, FMOD Studio `2.03.14`, and FMOD for Unity `2.03.14`.
2. Check whether the FMOD menu and official FMOD components are present. If they are, reconnect your own FMOD Studio project or bank directory in **FMOD > Edit Settings**, rebuild your banks, and refresh Unity before attempting reintegration.
3. If the FMOD menu/components are genuinely missing or Unity reports FMOD compilation errors, close Unity and FMOD Studio.
4. Follow the institution-approved recovery method for the exact FMOD for Unity `2.03.14` integration. Do not delete project content or install another version without lecturer guidance.
5. Reopen the project in Unity `6000.0.78f1` and allow import/recompilation to finish.
6. Reconnect only your own FMOD Studio project or bank directory, rebuild banks, refresh events, and complete the verification in [Setting Up FMOD After Downloading the Project](FMODSetupGuide.md#you-are-ready-when).

This recovery procedure does not provide, copy, or require lecturer reference banks or event assignments.

## Quick checks

| Problem | First check |
| --- | --- |
| Game is silent | Your events contain audio, belong to built banks, are assigned to supplied sources, and Music/SFX are enabled |
| An event is missing | Correct project/bank path, bank membership, successful build, and Event Browser refresh |
| One tower/enemy is silent | Edit its source prefab under `Assets/MSP603/Resources/StudentAuthoring/`, not a runtime clone |
| Parameters do not respond | Exact spelling, scope/type, labelled values, and the correct event instance |
| Settings do not affect the mix | Global slider parameter names, bus/VCA routing, and enable-switch mapping |

## Reporting a problem

Use **Report a Bug** in the Main Menu or pause menu. If the framework cannot open, use the MSP603 bug-report form provided in Canvas/VLE. Include the scene, action, expected result, actual result, reproducible steps, Unity version, FMOD version, and whether the failure occurs before or after building banks. Exclude passwords, credentials, and unnecessary personal data.
