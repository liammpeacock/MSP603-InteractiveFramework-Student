#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MSP603.EditorTools
{
    /// <summary>
    /// Small Editor-only selector and position editor for the production waypoint transforms.
    /// Unity's Scene tools provide visual editing; real gameplay remains the movement test.
    /// </summary>
    public sealed class RouteAuthoringWindow : EditorWindow
    {
        private const string MenuPath = "MSP603/Route Waypoint Editor";
        private const float MapAspect = 1.5f;
        private const float HalfWorldWidth = 12.3f;
        private const float HalfWorldDepth = 10.5f;
        private const float PointHitRadius = 10f;

        private readonly List<ResolvedRoute> _routes = new List<ResolvedRoute>();
        private readonly Dictionary<int, Vector3> _baseline = new Dictionary<int, Vector3>();
        private int _routeIndex;
        private int _waypointIndex;
        private float _nudgeAmount = 0.1f;
        private bool _showAdvanced;
        private Vector2 _scrollPosition;
        private int _dragRouteIndex = -1;
        private int _dragWaypointIndex = -1;
        private int _dragUndoGroup = -1;
        private bool _releaseHotControl;
        private bool _playModeTransitioning;
        private string _saveStatus;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            GetWindow<RouteAuthoringWindow>("Route Waypoint Editor").Show();
        }

        private void OnEnable()
        {
            RegisterCallbacks();
            Refresh();
            ScheduleRefresh();
        }

        private void OnFocus()
        {
            Refresh();
            ScheduleRefresh();
        }

        private void OnDisable()
        {
            EndMapDrag(false);
            EditorApplication.delayCall -= DelayedRefresh;
            UnregisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            UnregisterCallbacks();
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Undo.undoRedoPerformed += Refresh;
            Selection.selectionChanged += FollowUnitySelection;
        }

        private void UnregisterCallbacks()
        {
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Undo.undoRedoPerformed -= Refresh;
            Selection.selectionChanged -= FollowUnitySelection;
        }

        private void OnSceneChanged(Scene previous, Scene next)
        {
            ClearCachedRoutes();
            Refresh();
            ScheduleRefresh();
            Repaint();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (scene != SceneManager.GetActiveScene()) return;
            ClearCachedRoutes();
            Refresh();
            ScheduleRefresh();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            _playModeTransitioning = state == PlayModeStateChange.ExitingEditMode ||
                                     state == PlayModeStateChange.ExitingPlayMode;
            ClearCachedRoutes();
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
            {
                _playModeTransitioning = false;
                Refresh();
                ScheduleRefresh();
            }
            Repaint();
        }

        private void ClearCachedRoutes()
        {
            _routes.Clear();
            _baseline.Clear();
            _routeIndex = 0;
            _waypointIndex = 0;
            _saveStatus = null;
            EndMapDrag(false);
        }

        private void OnHierarchyChanged()
        {
            if (!_playModeTransitioning) Refresh();
        }

        private void ScheduleRefresh()
        {
            EditorApplication.delayCall -= DelayedRefresh;
            EditorApplication.delayCall += DelayedRefresh;
        }

        private void DelayedRefresh()
        {
            if (this != null) Refresh();
        }

        private void OnGUI()
        {
            if (_releaseHotControl)
            {
                GUIUtility.hotControl = 0;
                _releaseHotControl = false;
            }
            if (!RoutesAreValid()) Refresh();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawWindowContents();
            EditorGUILayout.EndScrollView();
        }

        private void DrawWindowContents()
        {
            EditorGUILayout.LabelField("MSP603 Route Waypoint Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Drag production waypoints directly over the level map, then test movement in the real game.",
                MessageType.Info);
            if (EditingDisabled)
                EditorGUILayout.HelpBox("PLAY MODE - route editing disabled. Exit Play Mode to make persistent waypoint changes.", MessageType.Warning);

            if (_routes.Count == 0)
            {
                EditorGUILayout.HelpBox("No production routes found. Open Level01, Level02, or Level03 and ensure EnemyPaths exists.", MessageType.Warning);
                if (GUILayout.Button("Refresh")) Refresh();
                return;
            }

            DrawRouteSelector();
            DrawWaypointSelector();
            DrawNavigation();
            DrawMapEditor();
            DrawPositionEditor();
            DrawNudgeControls();
            DrawBaselineControls();
            DrawSelectedDiagnostic();
            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "Advanced", true);
            if (_showAdvanced) DrawAdvanced();
            if (!string.IsNullOrEmpty(_saveStatus)) EditorGUILayout.LabelField(_saveStatus, EditorStyles.miniLabel);
        }

        private void DrawRouteSelector()
        {
            string[] routeNames = _routes.ConvertAll(route => route.Name).ToArray();
            int nextRoute = Mathf.Clamp(EditorGUILayout.Popup("Route", _routeIndex, routeNames), 0, _routes.Count - 1);
            if (nextRoute == _routeIndex) return;
            _routeIndex = nextRoute;
            _waypointIndex = 0;
            SelectWaypoint(_waypointIndex);
        }

        private void DrawWaypointSelector()
        {
            ResolvedRoute route = CurrentRoute;
            string[] labels = new string[route.Points.Count];
            for (int i = 0; i < labels.Length; i++) labels[i] = WaypointLabel(route, i);
            int nextWaypoint = Mathf.Clamp(EditorGUILayout.Popup("Waypoint", _waypointIndex, labels), 0, labels.Length - 1);
            if (nextWaypoint != _waypointIndex) SelectWaypoint(nextWaypoint);
            EditorGUILayout.LabelField("Production object", CurrentWaypoint.name);
        }

        private void DrawNavigation()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_waypointIndex == 0))
                {
                    if (GUILayout.Button("Previous", GUILayout.Height(28f))) SelectWaypoint(_waypointIndex - 1);
                }
                using (new EditorGUI.DisabledScope(_waypointIndex == CurrentRoute.Points.Count - 1))
                {
                    if (GUILayout.Button("Next", GUILayout.Height(28f))) SelectWaypoint(_waypointIndex + 1);
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Go to Spawn")) SelectWaypoint(0);
                if (GUILayout.Button("Go to Exit")) SelectWaypoint(CurrentRoute.Points.Count - 1);
            }
        }

        private void DrawPositionEditor()
        {
            Transform point = CurrentWaypoint;
            Vector3 position = point.position;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Position on route plane", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(EditingDisabled))
            {
                EditorGUI.BeginChangeCheck();
                float x = EditorGUILayout.FloatField("X", position.x);
                float z = EditorGUILayout.FloatField("Z", position.z);
                EditorGUILayout.LabelField("Y height (unchanged)", position.y.ToString("F3", CultureInfo.InvariantCulture));
                if (!EditorGUI.EndChangeCheck()) return;
                SetWaypointPosition(point, new Vector3(x, position.y, z), "Edit Route Waypoint Position");
            }
        }

        private void DrawNudgeControls()
        {
            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(EditingDisabled))
            {
                _nudgeAmount = Mathf.Max(0.001f, EditorGUILayout.FloatField("Nudge amount", _nudgeAmount));
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Left")) Nudge(Vector3.left);
                    if (GUILayout.Button("Right")) Nudge(Vector3.right);
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Down")) Nudge(Vector3.back);
                    if (GUILayout.Button("Up")) Nudge(Vector3.forward);
                }
            }
        }

        private void DrawBaselineControls()
        {
            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(EditingDisabled))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Capture Baseline")) CaptureBaseline();
                    using (new EditorGUI.DisabledScope(!HasSelectedBaseline))
                    {
                        if (GUILayout.Button("Revert Selected")) RevertSelected();
                    }
                }
            }
            EditorGUILayout.LabelField("Modified waypoints", CountModified().ToString(CultureInfo.InvariantCulture));
        }

        private void DrawSelectedDiagnostic()
        {
            if (_waypointIndex <= 0 || _waypointIndex >= CurrentRoute.Points.Count - 1) return;
            EditorGUILayout.LabelField("Turn angle", $"{TurnAngle(CurrentRoute, _waypointIndex):F1} degrees");
        }

        private void DrawAdvanced()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh")) Refresh();
                if (GUILayout.Button("Copy Route Dump")) CopyRouteDump();
                using (new EditorGUI.DisabledScope(EditingDisabled))
                {
                    if (GUILayout.Button("Save Scene")) SaveScene();
                }
            }
            EditorGUILayout.LabelField("Scene", SceneManager.GetActiveScene().name);
            EditorGUILayout.LabelField("Resolved points", CurrentRoute.Points.Count.ToString(CultureInfo.InvariantCulture));
        }

        private void SelectWaypoint(int index)
        {
            _waypointIndex = Mathf.Clamp(index, 0, CurrentRoute.Points.Count - 1);
            Transform point = CurrentWaypoint;
            Selection.activeTransform = point;
            Tools.current = Tool.Move;
            EditorGUIUtility.PingObject(point.gameObject);
            Repaint();
        }

        private void DrawMapEditor()
        {
            Texture2D mapTexture = LoadMapTexture(SceneManager.GetActiveScene().name);
            if (mapTexture == null)
            {
                EditorGUILayout.HelpBox("The map texture for this production scene could not be loaded.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Route map", EditorStyles.boldLabel);
            float mapWidth = Mathf.Max(120f, Mathf.Min(position.width - 32f, 900f));
            float mapHeight = mapWidth / MapAspect;
            Rect layoutRect = GUILayoutUtility.GetRect(10f, mapHeight, GUILayout.ExpandWidth(true));
            Rect mapRect = new Rect(layoutRect.x + (layoutRect.width - mapWidth) * 0.5f, layoutRect.y, mapWidth, mapHeight);
            GUI.DrawTexture(mapRect, mapTexture, ScaleMode.StretchToFill);
            DrawRouteOverlay(mapRect);
            HandleMapInput(mapRect);
        }

        private void DrawRouteOverlay(Rect mapRect)
        {
            ResolvedRoute route = CurrentRoute;
            Vector3[] linePoints = new Vector3[route.Points.Count];
            for (int i = 0; i < route.Points.Count; i++)
            {
                Transform point = route.Points[i];
                if (point == null) return;
                linePoints[i] = WorldToMap(point.position, mapRect);
            }

            Handles.BeginGUI();
            Handles.color = new Color(1f, 0.82f, 0.1f, 1f);
            Handles.DrawAAPolyLine(3f, linePoints);
            Handles.EndGUI();

            for (int i = 0; i < linePoints.Length; i++)
            {
                bool selected = i == _waypointIndex;
                float size = selected ? 16f : i == 0 || i == linePoints.Length - 1 ? 12f : 8f;
                Color colour = selected ? Color.cyan : i == 0 ? Color.green : i == linePoints.Length - 1 ? Color.red : Color.white;
                Rect pointRect = new Rect(linePoints[i].x - size * 0.5f, linePoints[i].y - size * 0.5f, size, size);
                EditorGUI.DrawRect(pointRect, colour);
            }

            Vector2 selectedPosition = linePoints[_waypointIndex];
            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.cyan }
            };
            GUI.Label(new Rect(selectedPosition.x + 10f, selectedPosition.y - 11f, 70f, 22f), WaypointLabel(route, _waypointIndex), labelStyle);
        }

        private void HandleMapInput(Rect mapRect)
        {
            if (EditingDisabled) return;
            Event current = Event.current;
            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            if (current.type == EventType.MouseDown && current.button == 0 && mapRect.Contains(current.mousePosition))
            {
                int hitIndex = FindWaypointAt(current.mousePosition, mapRect);
                if (hitIndex < 0) return;
                _dragRouteIndex = _routeIndex;
                _dragWaypointIndex = hitIndex;
                _dragUndoGroup = -1;
                GUIUtility.hotControl = controlId;
                SelectWaypoint(hitIndex);
                current.Use();
                return;
            }

            if (GUIUtility.hotControl != controlId) return;
            if (current.type == EventType.MouseDrag && current.button == 0)
            {
                if (!TryGetDraggedWaypoint(out Transform point))
                {
                    EndMapDrag();
                    return;
                }

                if (_dragUndoGroup < 0)
                {
                    Undo.IncrementCurrentGroup();
                    _dragUndoGroup = Undo.GetCurrentGroup();
                    Undo.SetCurrentGroupName("Drag Route Waypoint");
                    Undo.RegisterCompleteObjectUndo(point, "Drag Route Waypoint");
                }

                Vector3 position = MapToWorld(current.mousePosition, mapRect, point.position.y);
                ApplyWaypointPosition(point, position);
                Repaint();
                current.Use();
            }
            else if (current.rawType == EventType.MouseUp)
            {
                EndMapDrag();
                current.Use();
            }
        }

        private int FindWaypointAt(Vector2 mousePosition, Rect mapRect)
        {
            float bestDistance = PointHitRadius * PointHitRadius;
            int bestIndex = -1;
            for (int i = 0; i < CurrentRoute.Points.Count; i++)
            {
                Transform point = CurrentRoute.Points[i];
                if (point == null) continue;
                float distance = (WorldToMap(point.position, mapRect) - mousePosition).sqrMagnitude;
                if (distance > bestDistance) continue;
                bestDistance = distance;
                bestIndex = i;
            }
            return bestIndex;
        }

        private bool TryGetDraggedWaypoint(out Transform point)
        {
            point = null;
            if (_dragRouteIndex < 0 || _dragRouteIndex >= _routes.Count) return false;
            ResolvedRoute route = _routes[_dragRouteIndex];
            if (_dragWaypointIndex < 0 || _dragWaypointIndex >= route.Points.Count) return false;
            point = route.Points[_dragWaypointIndex];
            return point != null && point.gameObject.scene == SceneManager.GetActiveScene();
        }

        private void EndMapDrag(bool releaseHotControl = true)
        {
            if (_dragUndoGroup >= 0) Undo.CollapseUndoOperations(_dragUndoGroup);
            _dragRouteIndex = -1;
            _dragWaypointIndex = -1;
            _dragUndoGroup = -1;
            if (releaseHotControl)
            {
                GUIUtility.hotControl = 0;
                _releaseHotControl = false;
            }
            else
            {
                _releaseHotControl = true;
            }
        }

        // Matches the production LevelDefinition.MapPoint calibration exactly.
        private static Vector2 WorldToMap(Vector3 worldPosition, Rect mapRect)
        {
            float u = (worldPosition.x + HalfWorldWidth) / (HalfWorldWidth * 2f);
            float v = (HalfWorldDepth - worldPosition.z) / (HalfWorldDepth * 2f);
            return new Vector2(mapRect.x + u * mapRect.width, mapRect.y + v * mapRect.height);
        }

        private static Vector3 MapToWorld(Vector2 mapPosition, Rect mapRect, float height)
        {
            float u = Mathf.Clamp01((mapPosition.x - mapRect.x) / mapRect.width);
            float v = Mathf.Clamp01((mapPosition.y - mapRect.y) / mapRect.height);
            return new Vector3(
                Mathf.Lerp(-HalfWorldWidth, HalfWorldWidth, u),
                height,
                Mathf.Lerp(HalfWorldDepth, -HalfWorldDepth, v));
        }

        private static Texture2D LoadMapTexture(string sceneName)
        {
            string resource = sceneName switch
            {
                "Level01" => "Presentation/Level_1_Map",
                "Level02" => "Presentation/Level_2_Map",
                "Level03" => "Presentation/Level_3_Map",
                _ => null
            };
            return string.IsNullOrEmpty(resource) ? null : Resources.Load<Texture2D>(resource);
        }

        private void FollowUnitySelection()
        {
            SceneView.RepaintAll();
            Transform selected = Selection.activeTransform;
            if (selected == null) return;
            for (int routeIndex = 0; routeIndex < _routes.Count; routeIndex++)
            {
                int pointIndex = _routes[routeIndex].Points.IndexOf(selected);
                if (pointIndex < 0) continue;
                _routeIndex = routeIndex;
                _waypointIndex = pointIndex;
                Repaint();
                return;
            }
        }

        private void Nudge(Vector3 direction)
        {
            Transform point = CurrentWaypoint;
            SetWaypointPosition(point, point.position + direction * _nudgeAmount, "Nudge Route Waypoint");
        }

        private void SetWaypointPosition(Transform point, Vector3 position, string undoName)
        {
            if (EditingDisabled || point == null) return;
            Undo.RecordObject(point, undoName);
            ApplyWaypointPosition(point, position);
        }

        private void ApplyWaypointPosition(Transform point, Vector3 position)
        {
            if (EditingDisabled || point == null) return;
            point.position = position;
            EditorUtility.SetDirty(point);
            EditorSceneManager.MarkSceneDirty(point.gameObject.scene);
            _saveStatus = null;
            SceneView.RepaintAll();
        }

        private void CaptureBaseline()
        {
            _baseline.Clear();
            foreach (ResolvedRoute route in _routes)
            foreach (Transform point in route.Points)
                if (point != null) _baseline[point.GetInstanceID()] = point.position;
        }

        private void RevertSelected()
        {
            if (EditingDisabled) return;
            Transform point = CurrentWaypoint;
            if (!_baseline.TryGetValue(point.GetInstanceID(), out Vector3 position)) return;
            SetWaypointPosition(point, position, "Revert Route Waypoint Position");
        }

        private int CountModified()
        {
            int count = 0;
            HashSet<int> visited = new HashSet<int>();
            foreach (ResolvedRoute route in _routes)
            foreach (Transform point in route.Points)
            {
                if (point == null) continue;
                int id = point.GetInstanceID();
                if (visited.Add(id) && _baseline.TryGetValue(id, out Vector3 position) && point.position != position) count++;
            }
            return count;
        }

        private void Refresh()
        {
            if (_playModeTransitioning) return;
            Transform previouslySelected = Selection.activeTransform;
            _routes.Clear();
            Scene activeScene = SceneManager.GetActiveScene();
            GameObject root = FindEnemyPaths(activeScene);
            if (root == null)
            {
                Repaint();
                return;
            }

            if (activeScene.name == "Level02")
            {
                Transform shared = root.transform.Find("Level2_SharedPath");
                AddResolvedRoute("Left Route", root.transform.Find("Level2_LeftPath"), shared);
                AddResolvedRoute("Right Route", root.transform.Find("Level2_RightPath"), shared);
            }
            else
            {
                int index = 0;
                foreach (Transform routeRoot in root.transform)
                {
                    AddResolvedRoute(FriendlyRouteName(routeRoot.name, index++), routeRoot, null);
                }
            }

            _routeIndex = Mathf.Clamp(_routeIndex, 0, Mathf.Max(0, _routes.Count - 1));
            if (_routes.Count > 0)
            {
                _waypointIndex = Mathf.Clamp(_waypointIndex, 0, _routes[_routeIndex].Points.Count - 1);
                RestoreSelectionIndex(previouslySelected);
            }
            Repaint();
            SceneView.RepaintAll();
        }

        private bool RoutesAreValid()
        {
            if (_playModeTransitioning) return _routes.Count == 0;
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded) return _routes.Count == 0;
            GameObject root = FindEnemyPaths(activeScene);
            if (root == null) return _routes.Count == 0;
            if (_routes.Count == 0) return false;
            foreach (ResolvedRoute route in _routes)
            foreach (Transform point in route.Points)
            {
                if (point == null || point.gameObject.scene != activeScene) return false;
            }
            return true;
        }

        private static GameObject FindEnemyPaths(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindNamedTransform(root.transform, "EnemyPaths");
                if (found != null) return found.gameObject;
            }
            return null;
        }

        private static Transform FindNamedTransform(Transform current, string targetName)
        {
            if (current.name == targetName) return current;
            foreach (Transform child in current)
            {
                Transform found = FindNamedTransform(child, targetName);
                if (found != null) return found;
            }
            return null;
        }

        private void RestoreSelectionIndex(Transform selected)
        {
            if (selected == null) return;
            for (int routeIndex = 0; routeIndex < _routes.Count; routeIndex++)
            {
                int pointIndex = _routes[routeIndex].Points.IndexOf(selected);
                if (pointIndex < 0) continue;
                _routeIndex = routeIndex;
                _waypointIndex = pointIndex;
                return;
            }
        }

        private void AddResolvedRoute(string name, Transform routeRoot, Transform sharedRoot)
        {
            if (routeRoot == null || routeRoot.childCount < 2) return;
            ResolvedRoute route = new ResolvedRoute(name);
            foreach (Transform point in routeRoot) route.Points.Add(point);
            if (sharedRoot != null)
            {
                foreach (Transform point in sharedRoot)
                {
                    if (route.Points.Count == 0 || route.Points[route.Points.Count - 1].position != point.position) route.Points.Add(point);
                }
            }
            if (route.Points.Count >= 2) _routes.Add(route);
        }

        private static string FriendlyRouteName(string sourceName, int index)
        {
            if (sourceName.IndexOf("Left", StringComparison.OrdinalIgnoreCase) >= 0) return "Left Route";
            if (sourceName.IndexOf("Right", StringComparison.OrdinalIgnoreCase) >= 0) return "Right Route";
            return index == 0 ? "Route" : sourceName;
        }

        private static string WaypointLabel(ResolvedRoute route, int index)
        {
            if (index == 0) return "SPAWN";
            if (index == route.Points.Count - 1) return "EXIT";
            return index.ToString("00", CultureInfo.InvariantCulture);
        }

        private static float TurnAngle(ResolvedRoute route, int index)
        {
            Vector3 incoming = route.Points[index].position - route.Points[index - 1].position;
            Vector3 outgoing = route.Points[index + 1].position - route.Points[index].position;
            return incoming.sqrMagnitude < 0.0001f || outgoing.sqrMagnitude < 0.0001f ? 0f : Vector3.Angle(incoming, outgoing);
        }

        private void CopyRouteDump()
        {
            ResolvedRoute route = CurrentRoute;
            StringBuilder report = new StringBuilder();
            report.AppendLine("MSP603 RESOLVED ROUTE");
            report.AppendLine($"Level: {SceneManager.GetActiveScene().name}");
            report.AppendLine($"Route: {route.Name}");
            report.AppendLine("Order | Role | Object | X | Y | Z | Turn");
            for (int i = 0; i < route.Points.Count; i++)
            {
                Transform point = route.Points[i];
                string role = i == 0 ? "SPAWN" : i == route.Points.Count - 1 ? "EXIT" : "WAYPOINT";
                string angle = i > 0 && i < route.Points.Count - 1 ? TurnAngle(route, i).ToString("F1", CultureInfo.InvariantCulture) : "-";
                report.AppendLine($"{i:00} | {role} | {point.name} | {point.position.x:F3} | {point.position.y:F3} | {point.position.z:F3} | {angle}");
            }
            EditorGUIUtility.systemCopyBuffer = report.ToString();
            Debug.Log(report + "Copied to clipboard.");
        }

        private void SaveScene()
        {
            if (EditingDisabled || _routes.Count == 0) return;
            Transform point = CurrentWaypoint;
            if (point == null) return;
            Scene scene = point.gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(scene.path)) return;
            if (EditorSceneManager.SaveScene(scene, scene.path, false))
            {
                _saveStatus = $"Saved {scene.name}";
                Repaint();
            }
        }

        private ResolvedRoute CurrentRoute => _routes[_routeIndex];
        private Transform CurrentWaypoint => CurrentRoute.Points[_waypointIndex];
        private bool HasSelectedBaseline => CurrentWaypoint != null && _baseline.ContainsKey(CurrentWaypoint.GetInstanceID());
        private static bool EditingDisabled => EditorApplication.isPlayingOrWillChangePlaymode;

        private sealed class ResolvedRoute
        {
            public ResolvedRoute(string name)
            {
                Name = name;
            }

            public string Name { get; }
            public List<Transform> Points { get; } = new List<Transform>();
        }
    }
}
#endif
