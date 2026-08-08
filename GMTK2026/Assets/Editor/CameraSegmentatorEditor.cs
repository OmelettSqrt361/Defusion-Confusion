#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

// One-stop workflow for the whole level: create/rename/delete segments,
// tune priority/pull, and reshape every box + trigger collider - all from
// the CameraSegmentator's inspector, with the shapes visible and draggable
// directly in the Scene view. You should rarely need to click into a child
// segment object anymore.
[CustomEditor(typeof(CameraSegmentator))]
public class CameraSegmentatorEditor : Editor
{
    bool editCollidersInScene = true;
    readonly Dictionary<CameraSegment, bool> expanded = new Dictionary<CameraSegment, bool>();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var segmentator = (CameraSegmentator)target;

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Segments"))
        {
            segmentator.RefreshSegments();
            EditorUtility.SetDirty(segmentator);
        }
        if (GUILayout.Button("+ Add Segment"))
        {
            var root = CameraSegmentFactory.CreateSegment(segmentator.transform);
            segmentator.RefreshSegments();
            Selection.activeGameObject = root;
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();

        editCollidersInScene = EditorGUILayout.ToggleLeft("Edit colliders in Scene View", editCollidersInScene);

        var segments = segmentator.GetComponentsInChildren<CameraSegment>(true);
        if (segments.Length == 0)
        {
            EditorGUILayout.HelpBox("No segments yet - click + Add Segment above.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Segments", EditorStyles.boldLabel);

        foreach (var seg in segments)
        {
            DrawSegment(segmentator, seg);
        }
    }

    void DrawSegment(CameraSegmentator segmentator, CameraSegment seg)
    {
        if (!expanded.ContainsKey(seg)) expanded[seg] = false;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        expanded[seg] = EditorGUILayout.Foldout(expanded[seg], seg.isOn ? $"{seg.name}  (on)" : seg.name, true);
        if (GUILayout.Button("Select", GUILayout.Width(50))) Selection.activeGameObject = seg.gameObject;
        if (GUILayout.Button("Delete", GUILayout.Width(50)))
        {
            if (EditorUtility.DisplayDialog("Delete segment?", $"Delete '{seg.name}' and everything under it?", "Delete", "Cancel"))
            {
                Undo.DestroyObjectImmediate(seg.gameObject);
                segmentator.RefreshSegments();
                GUIUtility.ExitGUI();
            }
        }
        EditorGUILayout.EndHorizontal();

        if (expanded[seg])
        {
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.DelayedTextField("Name", seg.name);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(seg.gameObject, "Rename Segment");
                seg.gameObject.name = newName;
            }

            var so = new SerializedObject(seg);
            so.Update();
            EditorGUILayout.PropertyField(so.FindProperty("priority"));
            EditorGUILayout.PropertyField(so.FindProperty("camPull"));
            EditorGUILayout.PropertyField(so.FindProperty("overrideCamEdgePercent"));
            EditorGUILayout.PropertyField(so.FindProperty("preset"));
            so.ApplyModifiedProperties();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Bounds colliders", EditorStyles.miniBoldLabel);
            var boxes = seg.GetComponents<BoxCollider2D>().Where(b => b.usedByComposite).ToList();
            foreach (var box in boxes)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Box  {box.size.x:0.#} x {box.size.y:0.#}  @ {box.offset}");
                if (GUILayout.Button("x", GUILayout.Width(22)))
                {
                    Undo.DestroyObjectImmediate(box);
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+ Box Collider"))
            {
                var box = Undo.AddComponent<BoxCollider2D>(seg.gameObject);
                box.usedByComposite = true;
                box.size = new Vector2(5f, 5f);
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Trigger boundary (turns segment on/off)", EditorStyles.miniBoldLabel);
            if (seg.IsOnBoundary == null)
            {
                if (GUILayout.Button("+ Trigger Boundary"))
                {
                    AddTriggerBoundary(seg);
                    SceneView.RepaintAll();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Polygon  ({seg.IsOnBoundary.GetPath(0).Length} pts)");
                if (GUILayout.Button("+pt", GUILayout.Width(35))) { AddPolygonPoint(seg.IsOnBoundary); SceneView.RepaintAll(); }
                if (GUILayout.Button("-pt", GUILayout.Width(35))) { RemovePolygonPoint(seg.IsOnBoundary); SceneView.RepaintAll(); }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    static void AddTriggerBoundary(CameraSegment seg)
    {
        var triggerGO = new GameObject("Trigger");
        Undo.RegisterCreatedObjectUndo(triggerGO, "Add Trigger Boundary");
        triggerGO.transform.SetParent(seg.transform, false);
        var poly = Undo.AddComponent<PolygonCollider2D>(triggerGO);
        poly.isTrigger = true;

        var so = new SerializedObject(seg);
        so.FindProperty("isOnBoundary").objectReferenceValue = poly;
        so.ApplyModifiedProperties();
    }

    static void AddPolygonPoint(PolygonCollider2D poly)
    {
        Undo.RecordObject(poly, "Add Trigger Point");
        var points = poly.GetPath(0).ToList();
        Vector2 last = points.Count > 0 ? points[points.Count - 1] : Vector2.zero;
        points.Add(last + new Vector2(1f, 0f));
        poly.SetPath(0, points);
        EditorUtility.SetDirty(poly);
    }

    static void RemovePolygonPoint(PolygonCollider2D poly)
    {
        var points = poly.GetPath(0).ToList();
        if (points.Count <= 3) return;
        Undo.RecordObject(poly, "Remove Trigger Point");
        points.RemoveAt(points.Count - 1);
        poly.SetPath(0, points);
        EditorUtility.SetDirty(poly);
    }

    // ---- Scene view: drag box + polygon colliders directly in the viewport ----

    void OnSceneGUI()
    {
        if (!editCollidersInScene) return;

        var segmentator = (CameraSegmentator)target;
        foreach (var seg in segmentator.GetComponentsInChildren<CameraSegment>(true))
        {
            foreach (var box in seg.GetComponents<BoxCollider2D>())
            {
                if (box.usedByComposite) DrawBoxHandle(box);
            }
            if (seg.IsOnBoundary != null) DrawPolygonHandles(seg.IsOnBoundary);
        }
    }

    static void DrawBoxHandle(BoxCollider2D box)
    {
        var t = box.transform;
        Matrix4x4 handleMatrix = Matrix4x4.TRS(t.TransformPoint(box.offset), t.rotation, t.lossyScale);

        var boundsHandle = new BoxBoundsHandle();
        boundsHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
        boundsHandle.center = Vector3.zero;
        boundsHandle.size = new Vector3(box.size.x, box.size.y, 0f);

        using (new Handles.DrawingScope(handleMatrix))
        {
            EditorGUI.BeginChangeCheck();
            boundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(box, "Resize Box Collider");
                box.size = new Vector2(boundsHandle.size.x, boundsHandle.size.y);
                if (boundsHandle.center != Vector3.zero)
                    box.offset += (Vector2)boundsHandle.center;
                EditorUtility.SetDirty(box);
            }
        }
    }

    static void DrawPolygonHandles(PolygonCollider2D poly)
    {
        var t = poly.transform;
        for (int p = 0; p < poly.pathCount; p++)
        {
            var localPoints = poly.GetPath(p);
            var worldPoints = new Vector3[localPoints.Length];
            bool changed = false;

            for (int i = 0; i < localPoints.Length; i++)
            {
                Vector3 worldPos = t.TransformPoint(localPoints[i]);
                float size = HandleUtility.GetHandleSize(worldPos) * 0.08f;

                EditorGUI.BeginChangeCheck();
                Vector3 newWorldPos = Handles.FreeMoveHandle(worldPos, Quaternion.identity, size, Vector3.zero, Handles.DotHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    changed = true;
                    localPoints[i] = t.InverseTransformPoint(newWorldPos);
                    worldPos = newWorldPos;
                }
                worldPoints[i] = worldPos;
            }

            Handles.color = new Color(1f, 0.9f, 0.2f, 0.8f);
            for (int i = 0; i < worldPoints.Length; i++)
                Handles.DrawLine(worldPoints[i], worldPoints[(i + 1) % worldPoints.Length]);

            if (changed)
            {
                Undo.RecordObject(poly, "Edit Trigger Polygon");
                poly.SetPath(p, localPoints);
                EditorUtility.SetDirty(poly);
            }
        }
    }
}
#endif