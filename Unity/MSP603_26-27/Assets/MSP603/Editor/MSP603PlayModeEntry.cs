using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MSP603.TowerDefense.Editor
{
    /// <summary>
    /// Uses MainMenu as the safe Play entry only when an unrelated scene is open.
    /// Developers can still enter Play Mode directly from MainMenu or Level01-03.
    /// </summary>
    [InitializeOnLoad]
    public static class MSP603PlayModeEntry
    {
        private const string MainMenuPath = "Assets/MSP603/Scenes/MainMenu.unity";

        static MSP603PlayModeEntry()
        {
            EditorSceneManager.activeSceneChangedInEditMode += (_, _) => ConfigureStartScene();
            EditorApplication.delayCall += ConfigureStartScene;
        }

        [MenuItem("MSP603/Open Framework Folder")]
        private static void OpenFrameworkFolder()
        {
            Object frameworkFolder = AssetDatabase.LoadAssetAtPath<Object>("Assets/MSP603");
            Selection.activeObject = frameworkFolder;
            EditorGUIUtility.PingObject(frameworkFolder);
        }

        private static void ConfigureStartScene()
        {
            Scene active = SceneManager.GetActiveScene();
            EditorSceneManager.playModeStartScene = ShouldUseMainMenu(active.path, Application.isBatchMode)
                ? AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath) : null;
        }

        public static bool ShouldUseMainMenu(string scenePath, bool batchMode)
        {
            // Unity's generated scene contains the test runner itself. Replacing
            // it with MainMenu prevents any PlayMode tests/results from running.
            if (batchMode || scenePath.StartsWith("Assets/InitTestScene", System.StringComparison.Ordinal))
                return false;

            return scenePath != MainMenuPath &&
                scenePath != "Assets/MSP603/Scenes/Level01.unity" &&
                scenePath != "Assets/MSP603/Scenes/Level02.unity" &&
                scenePath != "Assets/MSP603/Scenes/Level03.unity";
        }
    }
}
