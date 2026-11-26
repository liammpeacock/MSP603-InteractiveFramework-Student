using FMODUnity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MSP603.TowerDefense.FMODIntegration.Editor
{
    public static class FMODTeachingSetup
    {
        private const string MainMenuPath = "Assets/MSP603/Scenes/MainMenu.unity";
        private const string TowerFireEventPath = "event:/Towers/Tower_Fire";
        private static readonly string[] LevelPaths =
        {
            "Assets/MSP603/Scenes/Level01.unity",
            "Assets/MSP603/Scenes/Level02.unity",
            "Assets/MSP603/Scenes/Level03.unity"
        };

        [MenuItem("MSP603/Configure FMOD Teaching Components")]
        public static void ConfigureScenes()
        {
            ConfigureMainMenu();
            foreach (string levelPath in LevelPaths) ConfigureLevel(levelPath);
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(MainMenuPath);
            Debug.Log("FMOD teaching components configured. Select meaningful objects under Student FMOD Authoring to assign student events.");
        }

        public static void ConfigureFromCommandLine()
        {
            ConfigureScenes();
        }

        public static void ValidateContinuousParametersFromCommandLine()
        {
            ValidateScene(MainMenuPath, false);
            foreach (string levelPath in LevelPaths) ValidateScene(levelPath, true);
            Debug.Log("MSP603 continuous FMOD parameter validation passed for all four scenes.");
        }

        private static void ConfigureMainMenu()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(MainMenuPath);
            AddListenerToMainCamera();
            GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
            EnsureComponent<FMODAudioBridge>(authoring);
            AddEmptyEmitter("Main Menu Music");
            AddEmptyEmitter("Main Menu Ambience");
            ConfigureVolumeSliders(false);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuPath);
        }

        private static void ConfigureLevel(string levelPath)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(levelPath);
            AddListenerToMainCamera();
            GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
            EnsureComponent<FMODAudioBridge>(authoring);
            StudioEventEmitter levelMusic = AddEmptyEmitter("Level Music");
            levelMusic.EventPlayTrigger = EmitterGameEvent.ObjectStart;
            AddEmptyEmitter("Level Ambience").EventPlayTrigger = EmitterGameEvent.ObjectStart;
            AddEnemyDamageAndDeathEmitters(authoring.transform);
            AddEnemyMovementEmitters(authoring.transform);
            AddEmptyEmitter("Player Life Lost");
            AddEmptyEmitter("Tower Placed");
            AddEmptyEmitter("Tower Upgraded");
            EnsureAuthoringEmitter(authoring.transform, "Tower Upgrade Failed");
            ConfigureTowerFireEmitter(authoring.transform);
            foreach (string control in new[] { "Restart", "Next", "Levels", "Home" })
            {
                EnsureComponent<StudioEventEmitter>(authoring.transform.Find("UI Control Sources/" + control).gameObject);
            }
            AddTriggerExamples(authoring.transform);
            ConfigureVolumeSliders(true);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, levelPath);
        }

        private static void AddListenerToMainCamera()
        {
            Camera camera = Camera.main;
            if (camera != null && camera.GetComponent<StudioListener>() == null)
            {
                camera.gameObject.AddComponent<StudioListener>();
            }
        }

        private static StudioEventEmitter AddEmptyEmitter(string objectName)
        {
            GameObject target = GameObject.Find(objectName);
            return target != null ? EnsureComponent<StudioEventEmitter>(target) : null;
        }

        private static StudioEventEmitter EnsureAuthoringEmitter(Transform authoring, string objectName)
        {
            Transform source = authoring.Find(objectName);
            if (source == null)
            {
                source = new GameObject(objectName).transform;
                source.SetParent(authoring, false);
            }
            return EnsureComponent<StudioEventEmitter>(source.gameObject);
        }

        private static void ConfigureTowerFireEmitter(Transform authoring)
        {
            StudioEventEmitter emitter = EnsureAuthoringEmitter(authoring, "Tower Fire");
            emitter.EventReference = EventReference.Find(TowerFireEventPath);
            emitter.Params = new[] { new ParamRef { Name = "TowerType", Value = 0f } };
        }

        private static void AddEnemyDamageAndDeathEmitters(Transform authoring)
        {
            Transform parent = authoring.Find("Enemy Damage and Death");
            if (parent == null) throw new System.InvalidOperationException("Missing Enemy Damage and Death authoring source");

            foreach (string enemyName in new[] { "Goblin Raider", "Goblin Brute", "Ruin Knight" })
            {
                Transform enemy = parent.Find(enemyName);
                if (enemy == null)
                {
                    enemy = new GameObject(enemyName).transform;
                    enemy.SetParent(parent, false);
                }

                foreach (string action in new[] { "Hit", "Death" })
                {
                    Transform source = enemy.Find(action);
                    if (source == null)
                    {
                        source = new GameObject(action).transform;
                        source.SetParent(enemy, false);
                    }
                    EnsureComponent<StudioEventEmitter>(source.gameObject);
                }
            }
        }

        private static void ConfigureVolumeSliders(bool gameplay)
        {
            GameObject root = GameObject.Find(StudentAuthoringTemplates.RootName);
            string prefix = "UI Control Sources/";
            ConfigureSlider(root.transform.Find(prefix + (gameplay ? "MASTER Slider" : "MASTER VOLUME Slider")), "RTPC_Master_Volume");
            ConfigureSlider(root.transform.Find(prefix + (gameplay ? "MUSIC Slider" : "MUSIC VOLUME Slider")), "RTPC_Music_Volume");
            ConfigureSlider(root.transform.Find(prefix + (gameplay ? "SFX Slider" : "SFX VOLUME Slider")), "RTPC_SFX_Volume");
        }

        private static void AddEnemyMovementEmitters(Transform authoring)
        {
            Transform parent = authoring.Find("Enemy Movement");
            if (parent == null)
            {
                parent = new GameObject("Enemy Movement").transform;
                parent.SetParent(authoring, false);
            }

            foreach (string sourceName in new[]
                     {
                         "Goblin Raider Movement", "Goblin Brute Movement", "Ruin Knight Movement"
                     })
            {
                Transform source = parent.Find(sourceName);
                if (source == null)
                {
                    source = new GameObject(sourceName).transform;
                    source.SetParent(parent, false);
                }
                EnsureComponent<StudioEventEmitter>(source.gameObject);
            }
        }

        private static void AddTriggerExamples(Transform authoring)
        {
            Transform wave = authoring.Find("Wave Start and End");
            Transform enemy = authoring.Find("Enemy Damage and Death");
            if (wave != null) EnsureComponent<StudioGlobalParameterTrigger>(wave.gameObject);
            if (enemy != null) EnsureComponent<StudioParameterTrigger>(enemy.gameObject);
        }

        private static void ConfigureSlider(Transform source, string parameterName)
        {
            if (source == null) throw new System.InvalidOperationException("Missing student volume slider source for " + parameterName);
            Slider slider = source.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.wholeNumbers = false;
            slider.value = 100f;
            MSP603FMODGlobalParameterSlider bridge = EnsureComponent<MSP603FMODGlobalParameterSlider>(source.gameObject);
            bridge.Configure(slider, parameterName);
        }

        private static void ValidateScene(string scenePath, bool gameplay)
        {
            EditorSceneManager.OpenScene(scenePath);
            GameObject root = GameObject.Find(StudentAuthoringTemplates.RootName);
            if (root == null) throw new System.InvalidOperationException("Missing Student FMOD Authoring in " + scenePath);

            string prefix = "UI Control Sources/";
            ValidateSlider(root.transform.Find(prefix + (gameplay ? "MASTER Slider" : "MASTER VOLUME Slider")), "RTPC_Master_Volume");
            ValidateSlider(root.transform.Find(prefix + (gameplay ? "MUSIC Slider" : "MUSIC VOLUME Slider")), "RTPC_Music_Volume");
            ValidateSlider(root.transform.Find(prefix + (gameplay ? "SFX Slider" : "SFX VOLUME Slider")), "RTPC_SFX_Volume");

            if (gameplay && (root.GetComponentInChildren<StudioGlobalParameterTrigger>(true) == null ||
                             root.GetComponentInChildren<StudioParameterTrigger>(true) == null))
                throw new System.InvalidOperationException("Missing official FMOD trigger examples in " + scenePath);
        }

        private static void ValidateSlider(Transform source, string expectedName)
        {
            if (source == null) throw new System.InvalidOperationException("Missing slider source for " + expectedName);
            Slider slider = source.GetComponent<Slider>();
            MSP603FMODGlobalParameterSlider bridge = source.GetComponent<MSP603FMODGlobalParameterSlider>();
            if (slider == null || bridge == null || bridge.GlobalParameterName != expectedName ||
                slider.minValue != 0f || slider.maxValue != 100f || slider.wholeNumbers || slider.value != 100f)
                throw new System.InvalidOperationException("Invalid continuous parameter contract for " + expectedName);

            slider.value = 63.7f;
            if (Mathf.Abs(slider.value - 63.7f) > .001f)
                throw new System.InvalidOperationException("Slider quantised an intermediate value for " + expectedName);
            slider.value = 100f;
        }

        private static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            T existing = gameObject.GetComponent<T>();
            return existing != null ? existing : gameObject.AddComponent<T>();
        }
    }
}
