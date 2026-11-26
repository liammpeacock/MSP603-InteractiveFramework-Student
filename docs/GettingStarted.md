# Getting started — Student v2.0.0

Use the version specified in your VLE; v2.0.0 is an optional upgrade. The original [v1.1.0 ZIP](https://github.com/liammpeacock/MSP603-InteractiveFramework-Student/releases/download/v1.1.0/MSP603-InteractiveFramework-Student-v1.1.0.zip) remains available.

For v2, extract its versioned ZIP into a new writable local folder and keep Unity and Audio together. Add `Unity/MSP603_26-27` in Unity Hub, use Unity 6000.0.78f1, allow import/package resolution to finish, then open `Assets/MSP603/Scenes/MainMenu.unity` and enter Play Mode. Test all three levels and their initial 20 lives.

The supplied matching FMOD authoring project retains event identities, parameters, routing, Unity assignments and freshly built banks. It contains no authored audio; silence is expected until you add your own sound design and rebuild banks. Follow [FMOD setup](FMODSetupGuide.md) and [required software](RequiredSoftware.md).

Back up your current assessment project first. Never extract over it or copy an entire replacement Unity/FMOD integration over existing work. See [migration guidance](StudentMigration.md).
