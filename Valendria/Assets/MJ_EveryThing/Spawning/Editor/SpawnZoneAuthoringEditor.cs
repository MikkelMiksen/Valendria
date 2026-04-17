#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(SpawnZoneAuthoring))]
public class SpawnZoneAuthoringEditor : Editor
{
    private bool _isDrawing = false;

    public override void OnInspectorGUI()
    {
        SpawnZoneAuthoring authoring = (SpawnZoneAuthoring)target;

        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (!_isDrawing)
        {
            if (GUILayout.Button("Start Drawing"))
            {
                _isDrawing = true;
            }
        }
        else
        {
            if (GUILayout.Button("Stop Drawing"))
            {
                _isDrawing = false;
            }
        }

        if (GUILayout.Button("Clear All Polygons"))
        {
            Undo.RecordObject(authoring, "Clear All Polygons");
            authoring.polygons.Clear();
            EditorUtility.SetDirty(authoring);
        }

        if (_isDrawing)
        {
            EditorGUILayout.HelpBox("Shift + LMB: Add points to current polygon.\nShift + RMB: Finish current polygon and start a new one.\nPress Stop Drawing when done.", MessageType.Info);
        }
    }

    private void OnSceneGUI()
    {
        SpawnZoneAuthoring authoring = (SpawnZoneAuthoring)target;

        if (!_isDrawing)
        {
            DrawAllPolygons(authoring);
            return;
        }

        Event e = Event.current;

        // Prevent deselecting while drawing
        if (e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        // Shift + RMB to finish current polygon (basically ensures next Shift+LMB starts a new list)
        if (e.type == EventType.MouseDown && e.button == 1 && e.shift)
        {
            if (authoring.polygons.Count > 0 && authoring.polygons[authoring.polygons.Count - 1].points.Count > 0)
            {
                Undo.RecordObject(authoring, "Finish Polygon");
                authoring.polygons.Add(new PolygonData());
                EditorUtility.SetDirty(authoring);
                e.Use();
            }
        }

        // Shift + LMB to add point
        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3? hitPoint = null;

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hitPoint = hit.point;
            }
            else
            {
                // Fallback to a plane if no collider is hit
                Plane plane = new Plane(Vector3.up, authoring.transform.position);
                if (plane.Raycast(ray, out float enter))
                {
                    hitPoint = ray.GetPoint(enter);
                }
            }

            if (hitPoint.HasValue)
            {
                Undo.RecordObject(authoring, "Add Point");
                if (authoring.polygons.Count == 0)
                {
                    authoring.polygons.Add(new PolygonData());
                }
                
                // If the last polygon was "finished" but empty, we just use it.
                authoring.polygons[authoring.polygons.Count - 1].points.Add(hitPoint.Value);
                EditorUtility.SetDirty(authoring);
                e.Use();
            }
        }

        DrawAllPolygons(authoring);
        
        // Draw handles for moving existing points in all polygons
        for (int pIdx = 0; pIdx < authoring.polygons.Count; pIdx++)
        {
            var polygon = authoring.polygons[pIdx];
            for (int i = 0; i < polygon.points.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(polygon.points[i], Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(authoring, "Move Point");
                    polygon.points[i] = newPos;
                    EditorUtility.SetDirty(authoring);
                }
            }
        }
    }

    private void DrawAllPolygons(SpawnZoneAuthoring authoring)
    {
        if (authoring.polygons == null) return;

        foreach (var polygon in authoring.polygons)
        {
            DrawPolygon(polygon.points);
        }
    }

    private void DrawPolygon(List<Vector3> points)
    {
        if (points == null || points.Count < 2) return;

        Handles.color = Color.cyan;
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 p1 = points[i];
            Vector3 p2 = points[(i + 1) % points.Count];
            Handles.DrawLine(p1, p2, 2f);
        }

        // Fill the polygon with a transparent color
        if (points.Count >= 3)
        {
            Handles.color = new Color(0, 1, 1, 0.2f);
            Handles.DrawAAConvexPolygon(points.ToArray());
        }
    }
}
#endif
