using UnityEngine;

namespace MSP603.TowerDefense
{
    /// <summary>Finds scene-authored UI sources that students may extend with official FMOD components.</summary>
    public static class StudentAuthoringTemplates
    {
        public const string RootName = "Student FMOD Authoring";

        public static T InstantiateUiSource<T>(string sourceName, Transform parent) where T : Component
        {
            T[] candidates = Resources.FindObjectsOfTypeAll<T>();
            foreach (T candidate in candidates)
            {
                if (candidate.name != sourceName || !candidate.gameObject.scene.IsValid() ||
                    candidate.transform.root.name != RootName)
                {
                    continue;
                }

                GameObject instance = Object.Instantiate(candidate.gameObject, parent, false);
                instance.name = sourceName;
                instance.SetActive(true);
                return instance.GetComponent<T>();
            }

            return null;
        }

        public static GameObject InstantiatePrefab(string resourcePath, string instanceName)
        {
            GameObject source = Resources.Load<GameObject>($"StudentAuthoring/{resourcePath}");
            if (source == null) return null;
            GameObject instance = Object.Instantiate(source);
            instance.name = instanceName;
            instance.SetActive(true);
            return instance;
        }
    }
}
