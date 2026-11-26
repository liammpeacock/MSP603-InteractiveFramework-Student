#if UNITY_EDITOR
using MSP603.TowerDefense;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MSP603.EditorTools
{
    /// <summary>
    /// Temporary, editor-only placement instrumentation for manually placed towers.
    /// Remove this file when the placement investigation is complete.
    /// </summary>
    [InitializeOnLoad]
    internal static class TowerPlacementDiagnosticOverlay
    {
        private const string EnabledPreference = "MSP603.TowerPlacementDiagnosticOverlay.Enabled";
        private const string MenuPath = "MSP603/Diagnostics/Tower Placement Overlay";

        private static readonly Color BuildColour = new Color(0f, 0.9f, 1f, 1f);
        private static readonly Color RootColour = new Color(0.1f, 1f, 0.2f, 1f);
        private static readonly Color PivotColour = new Color(1f, 0.15f, 1f, 1f);
        private static readonly Color BoundsColour = new Color(1f, 0.75f, 0.05f, 1f);

        static TowerPlacementDiagnosticOverlay()
        {
            SceneView.duringSceneGui += DrawOverlay;
            EditorApplication.playModeStateChanged += _ => SceneView.RepaintAll();
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            Enabled = !Enabled;
            SceneView.RepaintAll();
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateToggle()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return true;
        }

        private static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledPreference, true);
            set => EditorPrefs.SetBool(EnabledPreference, value);
        }

        private static void DrawOverlay(SceneView sceneView)
        {
            if (!Enabled || !Application.isPlaying || Event.current.type != EventType.Repaint)
            {
                return;
            }

            Tower[] towers = Object.FindObjectsByType<Tower>(FindObjectsSortMode.None);
            CompareFunction previousZTest = Handles.zTest;
            Handles.zTest = CompareFunction.Always;

            foreach (Tower tower in towers)
            {
                if (tower == null)
                {
                    continue;
                }

                DrawTower(sceneView, tower);
            }

            Handles.zTest = previousZTest;
        }

        private static void DrawTower(SceneView sceneView, Tower tower)
        {
            Vector3 root = tower.transform.position;
            BuildPlot plot = FindOwningPlot(tower);
            if (plot != null)
            {
                DrawCross(plot.transform.position, BuildColour, 0.32f);
                DrawLabel(sceneView, plot.transform.position, "BUILD", BuildColour, 0);
            }

            DrawPoint(root, RootColour, 0.20f);
            DrawLabel(sceneView, root, "ROOT", RootColour, 1);

            Transform visual = tower.transform.Find("AnimatedTowerVisual");
            if (visual == null)
            {
                return;
            }

            Vector3 pivot = visual.position;
            DrawPoint(pivot, PivotColour, 0.13f);
            DrawLabel(sceneView, pivot, "PIVOT", PivotColour, 2);

            SpriteRenderer spriteRenderer = visual.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            Bounds bounds = spriteRenderer.bounds;
            Handles.color = BoundsColour;
            Handles.DrawWireCube(bounds.center, bounds.size);

            Vector3 boundsBase = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            DrawCross(boundsBase, BoundsColour, 0.24f);
            DrawLabel(sceneView, boundsBase, "BOUNDS BASE", BoundsColour, 3);

            Handles.color = BoundsColour;
            Handles.DrawDottedLine(root, boundsBase, 4f);
        }

        private static BuildPlot FindOwningPlot(Tower tower)
        {
            BuildPlot[] plots = Object.FindObjectsByType<BuildPlot>(FindObjectsSortMode.None);
            foreach (BuildPlot plot in plots)
            {
                if (plot != null && plot.Tower == tower)
                {
                    return plot;
                }
            }

            return null;
        }

        private static void DrawPoint(Vector3 position, Color colour, float size)
        {
            Handles.color = colour;
            Handles.SphereHandleCap(0, position, Quaternion.identity, size, EventType.Repaint);
        }

        private static void DrawCross(Vector3 position, Color colour, float size)
        {
            Handles.color = colour;
            Handles.DrawLine(position - Vector3.right * size, position + Vector3.right * size, 2f);
            Handles.DrawLine(position - Vector3.up * size, position + Vector3.up * size, 2f);
            Handles.DrawLine(position - Vector3.forward * size, position + Vector3.forward * size, 2f);
        }

        private static void DrawLabel(SceneView sceneView, Vector3 position, string text, Color colour, int row)
        {
            Vector3 offset = sceneView.camera.transform.up * (0.32f + row * 0.22f);
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = colour;
            Handles.Label(position + offset, text + "  " + Format(position), style);
        }

        private static string Format(Vector3 position)
        {
            return $"({{position.x:F3}}, {{position.y:F3}}, {{position.z:F3}})";
        }
    }
}
#endif
