#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using MSP603.TowerDefense;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MSP603.EditorTools
{
    [InitializeOnLoad]
    internal static class RuntimeBuildPointAuthoring
    {
        private const string RootName = "TowerBuildPoints";
        private const string PendingKey = "MSP603.RuntimeBuildPointAuthoring.Pending";
        private const float LockedY = 0.18f;

        private static RuntimeBuildPointAuthoringOverlay _overlay;
        private static int _activeLevel;

        [Serializable]
        private sealed class PositionSet
        {
            public int levelNumber;
            public Vector3[] positions;
        }

        static RuntimeBuildPointAuthoring()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem("MSP603/Level 1/Runtime Build Point Authoring")]
        private static void ToggleLevel1() => ToggleAuthoring(1);

        [MenuItem("MSP603/Level 2/Runtime Build Point Authoring")]
        private static void ToggleLevel2() => ToggleAuthoring(2);

        [MenuItem("MSP603/Level 3/Runtime Build Point Authoring")]
        private static void ToggleLevel3() => ToggleAuthoring(3);

        private static void ToggleAuthoring(int levelNumber)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning($"Enter Play Mode in {SceneName(levelNumber)} before enabling runtime BuildPoint authoring.");
                return;
            }

            if (_overlay == null)
            {
                Enable(levelNumber);
            }
            else
            {
                Cancel();
            }
        }

        [MenuItem("MSP603/Level 1/Runtime Build Point Authoring", true)]
        private static bool ValidateLevel1() => ValidateToggleAuthoring(1);

        [MenuItem("MSP603/Level 2/Runtime Build Point Authoring", true)]
        private static bool ValidateLevel2() => ValidateToggleAuthoring(2);

        [MenuItem("MSP603/Level 3/Runtime Build Point Authoring", true)]
        private static bool ValidateLevel3() => ValidateToggleAuthoring(3);

        private static bool ValidateToggleAuthoring(int levelNumber)
        {
            Menu.SetChecked($"MSP603/Level {levelNumber}/Runtime Build Point Authoring", _overlay != null && _activeLevel == levelNumber);
            return true;
        }

        [MenuItem("MSP603/Level 1/Apply Runtime BuildPoint Positions")]
        private static void ApplyLevel1() => Apply(1);
        [MenuItem("MSP603/Level 2/Apply Runtime BuildPoint Positions")]
        private static void ApplyLevel2() => Apply(2);
        [MenuItem("MSP603/Level 3/Apply Runtime BuildPoint Positions")]
        private static void ApplyLevel3() => Apply(3);

        [MenuItem("MSP603/Level 1/Apply Runtime BuildPoint Positions", true)]
        private static bool ValidateApplyLevel1() => ValidateApply(1);
        [MenuItem("MSP603/Level 2/Apply Runtime BuildPoint Positions", true)]
        private static bool ValidateApplyLevel2() => ValidateApply(2);
        [MenuItem("MSP603/Level 3/Apply Runtime BuildPoint Positions", true)]
        private static bool ValidateApplyLevel3() => ValidateApply(3);

        private static bool ValidateApply(int levelNumber) =>
            EditorApplication.isPlaying && _overlay != null && _activeLevel == levelNumber;

        private static void Apply(int levelNumber)
        {
            if (_overlay == null || _activeLevel != levelNumber)
            {
                Debug.LogWarning($"Enable Level {levelNumber} Runtime Build Point Authoring during Play Mode first.");
                return;
            }

            Vector3[] positions = _overlay.CaptureLockedPositions();
            SessionState.SetString(PendingKey, JsonUtility.ToJson(new PositionSet { levelNumber = levelNumber, positions = positions }));
            _overlay.LockAfterApply();
            Debug.Log($"Captured {positions.Length} BuildPoint positions for {SceneName(levelNumber)}. Exit Play Mode to complete and verify the scene save.");
        }

        [MenuItem("MSP603/Level 1/Cancel Runtime BuildPoint Positions")]
        private static void CancelLevel1() => Cancel();
        [MenuItem("MSP603/Level 2/Cancel Runtime BuildPoint Positions")]
        private static void CancelLevel2() => Cancel();
        [MenuItem("MSP603/Level 3/Cancel Runtime BuildPoint Positions")]
        private static void CancelLevel3() => Cancel();

        private static void Cancel()
        {
            SessionState.EraseString(PendingKey);
            if (_overlay != null)
            {
                _overlay.CancelAndClose();
            }
        }

        [MenuItem("MSP603/Level 1/Copy Runtime BuildPoint Positions")]
        private static void CopyLevel1() => Copy(1);
        [MenuItem("MSP603/Level 2/Copy Runtime BuildPoint Positions")]
        private static void CopyLevel2() => Copy(2);
        [MenuItem("MSP603/Level 3/Copy Runtime BuildPoint Positions")]
        private static void CopyLevel3() => Copy(3);

        private static void Copy(int levelNumber)
        {
            Vector3[] positions = _overlay != null && _activeLevel == levelNumber
                ? _overlay.CaptureLockedPositions()
                : ReadLoadedBuildPoints(levelNumber);

            if (positions == null)
            {
                return;
            }

            EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(new PositionSet { levelNumber = levelNumber, positions = positions });
            Debug.Log($"Copied {positions.Length} {SceneName(levelNumber)} BuildPoint positions. Open the same level in the other Unity variant and use its matching Paste command.");
        }

        [MenuItem("MSP603/Level 1/Paste BuildPoint Positions")]
        private static void PasteLevel1() => Paste(1);
        [MenuItem("MSP603/Level 2/Paste BuildPoint Positions")]
        private static void PasteLevel2() => Paste(2);
        [MenuItem("MSP603/Level 3/Paste BuildPoint Positions")]
        private static void PasteLevel3() => Paste(3);

        private static void Paste(int levelNumber)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning($"Paste positions in Edit Mode so only the {SceneName(levelNumber)} scene asset is changed.");
                return;
            }

            PositionSet set;
            try
            {
                set = JsonUtility.FromJson<PositionSet>(EditorGUIUtility.systemCopyBuffer);
            }
            catch (Exception)
            {
                set = null;
            }

            if (set == null || set.levelNumber != levelNumber || set.positions == null || set.positions.Length == 0)
            {
                Debug.LogError($"Clipboard does not contain MSP603 {SceneName(levelNumber)} BuildPoint positions.");
                return;
            }

            ApplyToScene(levelNumber, set.positions);
        }

        private static void Enable(int levelNumber)
        {
            Transform[] points = FindRuntimeBuildPoints(levelNumber);
            Camera camera = Camera.main;
            if (points == null || points.Length != ExpectedPointCount(levelNumber) || camera == null)
            {
                Debug.LogError($"Runtime authoring requires {SceneName(levelNumber)}, exactly {ExpectedPointCount(levelNumber)} consecutively numbered BuildPoints, and the active runtime Main Camera.");
                return;
            }

            _activeLevel = levelNumber;
            _overlay = camera.gameObject.AddComponent<RuntimeBuildPointAuthoringOverlay>();
            _overlay.hideFlags = HideFlags.HideAndDontSave;
            _overlay.Initialise(points, camera, levelNumber, () => { _overlay = null; _activeLevel = 0; });
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _overlay = null;
                _activeLevel = 0;
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Scene restoration can still be completing during this callback. Waiting one editor
                // update prevents a runtime scene instance from being mistaken for editable scene data.
                EditorApplication.delayCall -= ApplyPendingPositions;
                EditorApplication.delayCall += ApplyPendingPositions;
            }
        }

        private static void ApplyPendingPositions()
        {
            string json = SessionState.GetString(PendingKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            PositionSet set = JsonUtility.FromJson<PositionSet>(json);
            if (set == null || set.levelNumber is < 1 or > 3 || set.positions == null || set.positions.Length == 0)
            {
                Debug.LogError("The pending runtime BuildPoint capture is invalid. No scene was modified.");
                return;
            }

            if (ApplyToScene(set.levelNumber, set.positions))
            {
                SessionState.EraseString(PendingKey);
            }
        }

        private static bool ApplyToScene(int levelNumber, Vector3[] positions)
        {
            string scenePath = ScenePath(levelNumber);
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedForApply = !scene.IsValid() || !scene.isLoaded;
            if (openedForApply)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            try
            {
                Transform[] points = FindBuildPoints(scene);
                int expectedCount = ExpectedPointCount(levelNumber);
                if (points == null || points.Length != expectedCount || positions.Length != expectedCount)
                {
                    Debug.LogError($"{SceneName(levelNumber)} must contain exactly {expectedCount} consecutively numbered BuildPoints matching the capture. No scene was modified.");
                    return false;
                }

                if (!openedForApply)
                {
                    Undo.RecordObjects(points, "Apply Runtime BuildPoint Positions");
                }

                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 local = positions[i];
                    local.y = LockedY;
                    points[i].localPosition = local;
                    EditorUtility.SetDirty(points[i]);
                }

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, scenePath))
                {
                    Debug.LogError($"Unity could not save {SceneName(levelNumber)}; the pending capture was retained.");
                    return false;
                }

                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 expected = positions[i];
                    expected.y = LockedY;
                    if (points[i].localPosition != expected)
                    {
                        Debug.LogError($"{SceneName(levelNumber)} save verification failed at {points[i].name}; the pending capture was retained.");
                        return false;
                    }
                }

                Debug.Log($"Applied, saved, and verified {points.Length} numbered BuildPoints in {scenePath}. Y was locked to {LockedY:F2}.");
                return true;
            }
            finally
            {
                if (openedForApply && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static Vector3[] ReadLoadedBuildPoints(int levelNumber)
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath(levelNumber));
            Transform[] points = scene.IsValid() && scene.isLoaded ? FindBuildPoints(scene) : null;
            if (points == null || points.Length != ExpectedPointCount(levelNumber))
            {
                Debug.LogError($"Load {SceneName(levelNumber)} with exactly {ExpectedPointCount(levelNumber)} consecutively numbered BuildPoints before copying positions.");
                return null;
            }

            Vector3[] positions = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                positions[i] = points[i].localPosition;
                positions[i].y = LockedY;
            }

            return positions;
        }

        private static Transform[] FindRuntimeBuildPoints(int levelNumber)
        {
            GameObject root = GameObject.Find(RootName);
            if (root == null || SceneManager.GetActiveScene().name != SceneName(levelNumber))
            {
                return null;
            }

            return FindChildren(root.transform);
        }

        private static Transform[] FindBuildPoints(Scene scene)
        {
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                Transform root = FindDescendant(rootObject.transform, RootName);
                if (root != null)
                {
                    return FindChildren(root);
                }
            }

            return null;
        }

        private static Transform FindDescendant(Transform current, string targetName)
        {
            if (current.name == targetName)
            {
                return current;
            }

            foreach (Transform child in current)
            {
                Transform match = FindDescendant(child, targetName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static Transform[] FindChildren(Transform root)
        {
            List<Transform> points = new List<Transform>();
            foreach (Transform child in root)
            {
                if (child.name.StartsWith("BuildPoint_", StringComparison.Ordinal))
                {
                    points.Add(child);
                }
            }

            points.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
            if (points.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < points.Count; i++)
            {
                if (!string.Equals(points[i].name, $"BuildPoint_{i + 1:00}", StringComparison.Ordinal))
                {
                    return null;
                }
            }

            return points.ToArray();
        }

        private static string SceneName(int levelNumber) => $"Level{levelNumber:00}";
        private static string ScenePath(int levelNumber) => $"Assets/MSP603/Scenes/{SceneName(levelNumber)}.unity";
        private static int ExpectedPointCount(int levelNumber) => levelNumber == 3 ? 9 : levelNumber == 2 ? 6 : 5;
    }

    internal sealed class RuntimeBuildPointAuthoringOverlay : MonoBehaviour
    {
        private const float LockedY = 0.18f;
        private Transform[] _points;
        private Vector3[] _startingLocalPositions;

        private Camera _camera;
        private Action _closed;
        private int _dragging = -1;
        private GUIStyle _handleStyle;
        private GUIStyle _statusStyle;
        private bool _applied;
        private int _levelNumber;

        public void Initialise(Transform[] points, Camera camera, int levelNumber, Action closed)
        {
            _points = points;
            _startingLocalPositions = new Vector3[points.Length];
            _camera = camera;
            _levelNumber = levelNumber;
            _closed = closed;
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = points[i];
                _startingLocalPositions[i] = points[i].localPosition;
                Vector3 local = points[i].localPosition;
                local.y = LockedY;
                points[i].localPosition = local;
            }
        }

        public Vector3[] CaptureLockedPositions()
        {
            Vector3[] result = new Vector3[_points.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = _points[i].localPosition;
                result[i].y = LockedY;
            }

            return result;
        }

        public void LockAfterApply()
        {
            _dragging = -1;
            _applied = true;
        }

        public void CancelAndClose()
        {
            for (int i = 0; i < _points.Length; i++)
            {
                if (_points[i] != null)
                {
                    _points[i].localPosition = _startingLocalPositions[i];
                }
            }

            _closed?.Invoke();
            Destroy(this);
        }

        private void OnGUI()
        {
            EnsureStyles();
            GUI.Box(new Rect(12f, 12f, 330f, 52f),
                _applied
                    ? "BUILDPOINT POSITIONS CAPTURED\nExit Play Mode to persist"
                    : $"LEVEL {_levelNumber} RUNTIME BUILDPOINT AUTHORING\nDrag numbered handles • Y locked to {LockedY:F2}",
                _statusStyle);

            Event current = Event.current;
            for (int i = 0; i < _points.Length; i++)
            {
                if (_points[i] == null)
                {
                    continue;
                }

                Vector3 screen = _camera.WorldToScreenPoint(_points[i].position);
                if (screen.z <= 0f)
                {
                    continue;
                }

                Vector2 gui = new Vector2(screen.x, Screen.height - screen.y);
                Rect handle = new Rect(gui.x - 24f, gui.y - 24f, 48f, 48f);
                GUI.Box(handle, (i + 1).ToString("00", CultureInfo.InvariantCulture), _handleStyle);

                if (!_applied && current.type == EventType.MouseDown && current.button == 0 && handle.Contains(current.mousePosition))
                {
                    _dragging = i;
                    current.Use();
                }
            }

            if (!_applied && _dragging >= 0 && current.type == EventType.MouseDrag && current.button == 0)
            {
                MoveDraggedPoint(current.mousePosition);
                current.Use();
            }
            else if (_dragging >= 0 && current.type == EventType.MouseUp && current.button == 0)
            {
                MoveDraggedPoint(current.mousePosition);
                _dragging = -1;
                current.Use();
            }
        }

        private void MoveDraggedPoint(Vector2 guiPosition)
        {
            Ray ray = _camera.ScreenPointToRay(new Vector3(guiPosition.x, Screen.height - guiPosition.y, 0f));
            Plane plane = new Plane(Vector3.up, new Vector3(0f, LockedY, 0f));
            if (!plane.Raycast(ray, out float distance))
            {
                return;
            }

            Transform point = _points[_dragging];
            Vector3 world = ray.GetPoint(distance);
            Vector3 local = point.parent != null ? point.parent.InverseTransformPoint(world) : world;
            local.y = LockedY;
            point.localPosition = local;
        }

        private void EnsureStyles()
        {
            if (_handleStyle != null)
            {
                return;
            }

            _handleStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            _handleStyle.normal.textColor = Color.black;
            _handleStyle.normal.background = Texture2D.whiteTexture;

            _statusStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
        }
    }
}
#endif
