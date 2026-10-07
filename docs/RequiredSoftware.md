# Required Software

Install this validated software set before opening the **MSP603 Interactive Framework v1.1.0**:

| Software | Required version |
| --- | --- |
| Unity Editor | `6000.0.78f1` |
| FMOD Studio | `2.03.13` |
| FMOD for Unity | `2.03.13` — pre-integrated in the approved Student project; reinstall only if integration recovery is required |

The Student project is a single platform-neutral Unity authoring package validated for **Windows/Intel x64** and **macOS Apple silicon**.

It is not a prebuilt executable.

Do not substitute newer Unity or FMOD versions unless your lecturer explicitly tells you to do so. Small version differences can create compatibility problems that are unrelated to your assessed audio work.

## Download Unity Hub and Unity 6000.0.78f1

1. Download and install **Unity Hub** from the official Unity site:
   - https://unity.com/download
2. Sign in to Unity Hub with your Unity ID.
3. Open Unity's official **Download Archive**:
   - https://unity.com/releases/editor/archive
4. Locate **Unity 6000.0.78f1**.
5. The specific release page is:
   - https://unity.com/releases/editor/whats-new/6000.0.78f1
6. On that page, click **Install**. If Unity Hub is already installed and you are signed in, the browser should hand the installation to Unity Hub.
7. If the Hub hand-off does not work, use the same release page and choose the correct manual installer for your operating system.

**Required editor version: `6000.0.78f1`. Do not install a different Unity 6 version for this project.**

## Download FMOD Studio 2.03.13

FMOD downloads require an FMOD account.

1. Go to the official FMOD download page:
   - https://www.fmod.com/download
2. Sign in or create a free FMOD account.
3. Find **FMOD Studio 2.03.13** in the available 2.03 downloads/older versions.
4. Download the installer for your operating system.

**Required FMOD Studio version: `2.03.13`. Do not simply install the newest FMOD Studio release.**

## FMOD for Unity 2.03.13

The approved Student package already contains the FMOD for Unity integration. You normally do **not** need to reinstall it.

If the integration is missing or damaged, use **FMOD for Unity 2.03.13**.

Official Unity Asset Store page:

- https://assetstore.unity.com/packages/package/1082237

The Asset Store page is for the **FMOD for Unity (2.03)** package and exposes multiple releases. Make sure you select **2.3.13 / 2.03.13**, not the current/latest version.

### Finding FMOD for Unity from inside Unity

If you need to recover the integration from within the Unity project:

1. Open the project in Unity `6000.0.78f1`.
2. Open **Window > Package Manager**.
3. Use the **My Assets** / Asset Store workflow available through Unity, or open the Unity Asset Store in your browser.
4. Search for **FMOD for Unity (2.03)** by **FMOD**.
5. Open the FMOD package page.
6. Choose release **2.3.13**.
7. Add/download/import that version only if the existing integration genuinely needs repairing.

If Unity offers a newer FMOD package by default, do not update the project to it.

## FMOD project

The package includes the supplied MSP603 Student FMOD Studio project.

You do **not** need to create a separate FMOD project for the assignment framework.

The supplied project is located at:

`Audio/FMOD/MSP603_26-27_TD/MSP603_26-27_TD.fspro`

Use the supplied project with FMOD Studio `2.03.13`.

## Installation and recovery

For first-time setup, integration checks or recovery, follow the [FMOD Setup Guide](FMODSetupGuide.md).

Do not substitute other Unity or FMOD versions without lecturer approval. Version differences can introduce integration or compatibility problems that are unrelated to your audio work.
