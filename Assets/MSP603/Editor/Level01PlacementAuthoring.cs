#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MSP603.EditorTools
{
    [InitializeOnLoad]
    internal static class Level01PlacementAuthoring
    {
        private const string LevelScenePath = "Assets/MSP603/Scenes/Level01.unity";
        private static readonly Color FillColour = new Color(1f, 0.72f, 0.08f, 0.35f);
        private static readonly Color OutlineColour = new Color(1f, 0.35f, 0.02f, 1f);
        private static GUIStyle _labelStyle;

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
        private static void DrawBuildPointGizmo(Transform transform, GizmoType gizmoType)
        {
            if (!IsLevel01() || !IsBuildPoint(transform)) return;

            bool selected = (gizmoType & GizmoType.Selected) != 0;
            Gizmos.color = selected ? Color.yellow : FillColour;
            Gizmos.DrawSphere(transform.position, selected ? 0.34f : 0.25f);
            Gizmos.color = OutlineColour;
            Gizmos.DrawWireSphere(transform.position, 0.55f);

            Handles.color = OutlineColour;
            Handles.DrawWireDisc(transform.position, Vector3.up, 0.75f, 2f);
            Handles.Label(
                transform.position + Vector3.up * 0.8f,
                transform.name.Substring(transform.name.Length - 2),
                GetLabelStyle());
        }

        private static bool IsLevel01()
        {
            return EditorSceneManager.GetActiveScene().path == LevelScenePath;
        }

        private static bool IsBuildPoint(Transform transform)
        {
            return transform != null &&
                   transform.parent != null &&
                   transform.parent.name == "TowerBuildPoints" &&
                   transform.name.StartsWith("BuildPoint_0");
        }

        private static GUIStyle GetLabelStyle()
        {
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                };
                _labelStyle.normal.textColor = Color.white;
            }

            return _labelStyle;
        }
    }
}
#endif
