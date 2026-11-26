using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MSP603.TowerDefense.Editor
{
    public static class VerticalSliceBuilder
    {
        private const string SceneDirectory = "Assets/MSP603/Scenes";
        private const string MainMenuPath = SceneDirectory + "/MainMenu.unity";
        private const string AuthoringPrefabDirectory = "Assets/MSP603/Resources/StudentAuthoring";
        private static readonly string[] LevelPaths =
        {
            SceneDirectory + "/Level01.unity",
            SceneDirectory + "/Level02.unity",
            SceneDirectory + "/Level03.unity"
        };

        [MenuItem("MSP603/Build Tower Defence Vertical Slice")]
        public static void Build()
        {
            if (!EditorUtility.DisplayDialog(
                    "Rebuild Tower Defence Vertical Slice?",
                    "This recreates MainMenu and all three level scenes and overwrites changes saved in those scenes.",
                    "Rebuild Scenes",
                    "Cancel"))
            {
                return;
            }

            BuildInternal();
        }

        private static void BuildInternal()
        {
            Directory.CreateDirectory(SceneDirectory);
            CreateAuthoringPrefabs();
            CreateMainMenu();
            foreach (string levelPath in LevelPaths) CreateLevel(levelPath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(LevelPaths[0], true),
                new EditorBuildSettingsScene(LevelPaths[1], true),
                new EditorBuildSettingsScene(LevelPaths[2], true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(MainMenuPath);
            Debug.Log("MSP603 vertical slice created. Press Play from MainMenu.");
        }

        public static void BuildFromCommandLine()
        {
            BuildInternal();
        }

        public static void ValidateStudentAuthoringFromCommandLine()
        {
            ValidateScene(MainMenuPath, false);
            foreach (string levelPath in LevelPaths) ValidateScene(levelPath, true);

            foreach (string path in new[]
            {
                "Towers/ArcherTower", "Towers/MageTower", "Towers/CannonTower", "Towers/InfernoTower",
                "Enemies/GoblinBrute", "Enemies/GoblinRaider", "Enemies/RuinKnight",
                "Projectiles/ArcherProjectile", "Projectiles/MageProjectile", "Projectiles/CannonProjectile", "Projectiles/InfernoFlame"
            })
            {
                if (Resources.Load<GameObject>("StudentAuthoring/" + path) == null)
                    throw new System.InvalidOperationException("Missing student authoring Prefab: " + path);
            }

            Debug.Log("MSP603 student FMOD authoring validation passed for four scenes and nine spawn Prefabs.");
        }

        private static void ValidateScene(string scenePath, bool gameplay)
        {
            EditorSceneManager.OpenScene(scenePath);
            GameObject root = GameObject.Find(StudentAuthoringTemplates.RootName);
            if (root == null) throw new System.InvalidOperationException($"Missing authoring root in {scenePath}");

            string[] required = gameplay
                ? new[] { "Level Music", "Level Ambience", "Wave Start and End", "Player Life Lost", "Victory", "Defeat", "Tower Upgraded", "Tower Upgrade Failed", "Tower Fire", "UI Control Sources/Pause", "UI Control Sources/Start Wave", "UI Control Sources/Report a Bug", "UI Control Sources/Upgrade", "UI Control Sources/Sell", "UI Control Sources/MUSIC Slider", "UI Control Sources/SFX Slider", "UI Control Sources/MUSIC ON", "UI Control Sources/SFX ON" }
                : new[] { "Main Menu Music", "Main Menu Ambience", "UI Control Sources/Play", "UI Control Sources/Level Select", "UI Control Sources/Settings", "UI Control Sources/Quit", "UI Control Sources/Report a Bug", "UI Control Sources/Back", "UI Control Sources/MUSIC VOLUME Slider", "UI Control Sources/SFX VOLUME Slider", "UI Control Sources/MUSIC ON", "UI Control Sources/SFX ON" };
            foreach (string path in required)
                if (root.transform.Find(path) == null) throw new System.InvalidOperationException($"Missing {path} in {scenePath}");
        }

        private static void CreateMainMenu()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            GameObject controller = new GameObject("MainMenuController");
            controller.AddComponent<MainMenuController>();
            CreateStudentAuthoring(false);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuPath);
        }

        private static void CreateLevel(string levelPath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            GameObject controller = new GameObject("TowerDefenseGame");
            controller.AddComponent<TowerDefenseGame>();
            CreateStudentAuthoring(true);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, levelPath);
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        private static void CreateStudentAuthoring(bool gameplay)
        {
            GameObject root = new GameObject(StudentAuthoringTemplates.RootName);
            CreateNamedChild(root.transform, gameplay ? "Level Music" : "Main Menu Music");
            CreateNamedChild(root.transform, gameplay ? "Level Ambience" : "Main Menu Ambience");

            if (gameplay)
            {
                foreach (string name in new[] { "Wave Start and End", "Player Life Lost", "Victory", "Defeat", "Tower Placed", "Tower Upgraded", "Tower Upgrade Failed", "Tower Fire", "Projectile Impact", "Enemy Damage and Death" })
                    CreateNamedChild(root.transform, name);
            }

            GameObject uiSources = CreateNamedChild(root.transform, "UI Control Sources");
            string[] buttons = gameplay
                ? new[] { "Pause", "Start Wave", "Resume", "Settings", "Restart", "Levels", "Home", "Report a Bug", "Back", "Archer", "Mage", "Cannon", "Inferno", "Upgrade", "Sell", "Next" }
                : new[] { "Play", "Level Select", "Settings", "Quit", "Report a Bug", "Back", "Level 1 Card", "Level 2 Card", "Level 3 Card" };
            foreach (string name in buttons) CreateUiButtonSource(uiSources.transform, name);

            string[] sliders = gameplay
                ? new[] { "MASTER Slider", "MUSIC Slider", "SFX Slider" }
                : new[] { "MASTER VOLUME Slider", "MUSIC VOLUME Slider", "SFX VOLUME Slider" };
            foreach (string name in sliders) CreateUiSliderSource(uiSources.transform, name);
            foreach (string name in new[] { "MUSIC ON", "SFX ON" }) CreateUiToggleSource(uiSources.transform, name);
            uiSources.SetActive(false);
        }

        private static GameObject CreateNamedChild(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void CreateUiButtonSource(Transform parent, string name)
        {
            GameObject source = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            source.transform.SetParent(parent, false);
        }

        private static void CreateUiSliderSource(Transform parent, string name)
        {
            GameObject source = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Slider));
            source.transform.SetParent(parent, false);
            UnityEngine.UI.Slider slider = source.GetComponent<UnityEngine.UI.Slider>();
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.wholeNumbers = false;
            slider.value = 100f;
        }

        private static void CreateUiToggleSource(Transform parent, string name)
        {
            GameObject source = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Toggle));
            source.transform.SetParent(parent, false);
        }

        private static void CreateAuthoringPrefabs()
        {
            EnsureAssetFolder(AuthoringPrefabDirectory);
            EnsureAssetFolder(AuthoringPrefabDirectory + "/Towers");
            EnsureAssetFolder(AuthoringPrefabDirectory + "/Enemies");
            EnsureAssetFolder(AuthoringPrefabDirectory + "/Projectiles");

            foreach (TowerType type in new[] { TowerType.Archer, TowerType.Mage, TowerType.Cannon, TowerType.Inferno })
            {
                string name = TowerCatalog.Get(type).Name;
                GameObject tower = new GameObject(name + "Tower");
                tower.AddComponent<Tower>();
                SavePrefab(tower, $"{AuthoringPrefabDirectory}/Towers/{name}Tower.prefab");

                if (type == TowerType.Inferno)
                {
                    GameObject flame = new GameObject("InfernoFlame");
                    flame.AddComponent<LineRenderer>();
                    flame.AddComponent<InfernoFlame>();
                    SavePrefab(flame, $"{AuthoringPrefabDirectory}/Projectiles/InfernoFlame.prefab");
                    continue;
                }

                PrimitiveType shape = type == TowerType.Archer ? PrimitiveType.Cube : PrimitiveType.Sphere;
                GameObject projectile = GameObject.CreatePrimitive(shape);
                projectile.name = name + "Projectile";
                projectile.AddComponent<Projectile>();
                SavePrefab(projectile, $"{AuthoringPrefabDirectory}/Projectiles/{name}Projectile.prefab");
            }

            foreach (EnemyType type in new[] { EnemyType.GoblinBrute, EnemyType.GoblinRaider, EnemyType.RuinKnight })
            {
                GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = type.ToString();
                enemy.AddComponent<EnemyUnit>();
                SavePrefab(enemy, $"{AuthoringPrefabDirectory}/Enemies/{type}.prefab");
            }
        }

        private static void SavePrefab(GameObject source, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(source, path);
            Object.DestroyImmediate(source);
        }

        private static void EnsureAssetFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
