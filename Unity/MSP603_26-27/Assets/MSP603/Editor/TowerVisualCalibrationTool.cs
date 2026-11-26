#if UNITY_EDITOR
using System;
using System.Globalization;
using MSP603.TowerDefense;
using UnityEditor;
using UnityEngine;

namespace MSP603.EditorTools
{
    [InitializeOnLoad]
    internal static class TowerVisualCalibrationTool
    {
        private const string MenuPath = "MSP603/Diagnostics/Tower Visual Calibration";
        private static TowerVisualCalibrationOverlay _overlay;

        static TowerVisualCalibrationTool()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("Tower Visual Calibration is available only during Play Mode.");
                return;
            }

            if (_overlay != null)
            {
                CloseAndReset();
                return;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("Tower Visual Calibration requires the active runtime Main Camera.");
                return;
            }

            _overlay = camera.gameObject.AddComponent<TowerVisualCalibrationOverlay>();
            _overlay.hideFlags = HideFlags.HideAndDontSave;
            _overlay.Closed += () => _overlay = null;
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateToggle()
        {
            Menu.SetChecked(MenuPath, _overlay != null);
            return true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode ||
                state == PlayModeStateChange.EnteredEditMode)
            {
                TowerVisualCalibration.ResetAll();
                _overlay = null;
            }
        }

        private static void CloseAndReset()
        {
            TowerVisualCalibration.ResetAll();
            if (_overlay != null)
            {
                _overlay.Close();
            }
        }
    }

    internal sealed class TowerVisualCalibrationOverlay : MonoBehaviour
    {
        private const int WindowId = 0x4D535056;
        private static readonly float[] Steps = { 0.01f, 0.05f, 0.10f };

        private Rect _window = new Rect(12f, 72f, 430f, 390f);
        private TowerType _selected = TowerType.Inferno;
        private int _stepIndex = 1;

        public event Action Closed;

        public void Close()
        {
            Closed?.Invoke();
            Destroy(this);
        }

        private void OnGUI()
        {
            _window = GUI.Window(
                WindowId,
                _window,
                DrawWindow,
                "TOWER VISUAL CALIBRATION — TEMPORARY");
        }

        private void DrawWindow(int id)
        {
            GUILayout.Label("Tower Type", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            DrawTowerButton(TowerType.Cannon);
            DrawTowerButton(TowerType.Archer);
            DrawTowerButton(TowerType.Mage);
            DrawTowerButton(TowerType.Inferno);
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);
            GUILayout.Label("Fine movement (screen/presentation plane)", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("↑  UP", GUILayout.Width(120f), GUILayout.Height(44f)))
            {
                Nudge(Vector2.up);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("←  LEFT", GUILayout.Height(44f)))
            {
                Nudge(Vector2.left);
            }
            if (GUILayout.Button("↓  DOWN", GUILayout.Height(44f)))
            {
                Nudge(Vector2.down);
            }
            if (GUILayout.Button("RIGHT  →", GUILayout.Height(44f)))
            {
                Nudge(Vector2.right);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);
            GUILayout.Label("Step size", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < Steps.Length; i++)
            {
                bool selected = i == _stepIndex;
                if (GUILayout.Toggle(selected, Steps[i].ToString("0.00", CultureInfo.InvariantCulture), "Button"))
                {
                    _stepIndex = i;
                }
            }
            GUILayout.EndHorizontal();

            Vector2 offset = TowerVisualCalibration.Get(_selected);
            Vector2 permanent = TowerVisualCalibration.GetPermanent(_selected);
            GUILayout.Space(10f);
            GUILayout.Label(
                $"Temporary Offset  X: {offset.x:+0.000;-0.000;0.000}    Y: {offset.y:+0.000;-0.000;0.000}",
                EditorStyles.boldLabel);
            GUILayout.Label(
                $"Permanent Baseline  X: {permanent.x:+0.000;-0.000;0.000}    Y: {permanent.y:+0.000;-0.000;0.000}");

            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Tower", GUILayout.Height(38f)))
            {
                TowerVisualCalibration.Reset(_selected);
            }
            if (GUILayout.Button("Capture Calibration", GUILayout.Height(38f)))
            {
                Capture();
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0f, 0f, _window.width, 24f));
        }

        private void DrawTowerButton(TowerType type)
        {
            bool selected = _selected == type;
            if (GUILayout.Toggle(selected, type.ToString(), "Button", GUILayout.Height(34f)))
            {
                _selected = type;
            }
        }

        private void Nudge(Vector2 direction)
        {
            Vector2 offset = TowerVisualCalibration.Get(_selected);
            TowerVisualCalibration.Set(_selected, offset + direction * Steps[_stepIndex]);
        }

        private void Capture()
        {
            Vector2 offset = TowerVisualCalibration.Get(_selected);
            Vector2 permanent = TowerVisualCalibration.GetPermanent(_selected);
            Vector2 combined = permanent + offset;
            string report =
                "MSP603 TOWER CALIBRATION\n" +
                _selected + "\n" +
                $"Temporary Horizontal: {offset.x:+0.000;-0.000;0.000}\n" +
                $"Temporary Vertical: {offset.y:+0.000;-0.000;0.000}\n" +
                $"Permanent Baseline: ({permanent.x:+0.000;-0.000;0.000}, {permanent.y:+0.000;-0.000;0.000})\n" +
                $"Combined: ({combined.x:+0.000;-0.000;0.000}, {combined.y:+0.000;-0.000;0.000})";

            EditorGUIUtility.systemCopyBuffer =
                $"{_selected} temporary: ({offset.x:+0.000;-0.000;0.000}, {offset.y:+0.000;-0.000;0.000}); " +
                $"combined: ({combined.x:+0.000;-0.000;0.000}, {combined.y:+0.000;-0.000;0.000})";
            Debug.Log(report + "\nCopied concise value to the clipboard.");
        }
    }
}
#endif
