#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Adds quick actions for shaping a segment's composite collider without
// leaving the inspector - "Add Box" for an L-shaped/irregular area, and a
// manual re-wire button for after you've moved things around.
[CustomEditor(typeof(CameraSegment))]
public class CameraSegmentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var seg = (CameraSegment)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Collider Shape", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Box"))
        {
            var box = Undo.AddComponent<BoxCollider2D>(seg.gameObject);
            box.usedByComposite = true;
            box.size = new Vector2(10f, 6f);
        }
        if (GUILayout.Button("Re-wire References"))
        {
            Undo.RecordObject(seg, "Re-wire Camera Segment");
            seg.AutoWire();
            EditorUtility.SetDirty(seg);
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif
