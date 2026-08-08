#if UNITY_EDITOR
using Cinemachine;
using UnityEditor;
using UnityEngine;

// Builds the full object structure that used to be assembled by hand:
//
//   Segment (Rigidbody2D static, CompositeCollider2D, BoxCollider2D usedByComposite)
//     +- VCam (CinemachineVirtualCamera + CinemachineConfiner)
//     +- Trigger (PolygonCollider2D, isTrigger)
//
// After creation just reshape the BoxCollider2D(s) and the Trigger polygon
// to the segment's actual area - everything else is already wired. Callable
// two ways: right-click a Hierarchy object -> Camera System > Create Camera
// Segment, or the "+ Add Segment" button in CameraSegmentator's inspector.
public static class CameraSegmentFactory
{
    [MenuItem("GameObject/Camera System/Create Camera Segment", false, 10)]
    static void CreateSegmentMenuItem(MenuCommand cmd)
    {
        var parentGO = cmd.context as GameObject;
        Transform parent = parentGO != null ? parentGO.transform : Selection.activeTransform;

        var root = CreateSegment(parent);

        var segmentator = root.GetComponentInParent<CameraSegmentator>();
        if (segmentator != null)
            segmentator.RefreshSegments();

        Selection.activeGameObject = root;
    }

    // The reusable creation logic - no selection/menu side effects, so it's
    // safe to call from inspector code too.
    public static GameObject CreateSegment(Transform parent)
    {
        var root = new GameObject("Segment");
        Undo.RegisterCreatedObjectUndo(root, "Create Camera Segment");
        if (parent != null)
            root.transform.SetParent(parent, false);

        var rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        root.AddComponent<CompositeCollider2D>();
        var box = root.AddComponent<BoxCollider2D>();
        box.usedByComposite = true;
        box.size = new Vector2(10f, 6f);

        var camGO = new GameObject("VCam");
        camGO.transform.SetParent(root.transform, false);
        var vcam = camGO.AddComponent<CinemachineVirtualCamera>();
        vcam.m_Lens.Orthographic = true;
        vcam.AddCinemachineComponent<CinemachineFramingTransposer>();
        var confiner = camGO.AddComponent<CinemachineConfiner>();
        confiner.m_ConfineMode = CinemachineConfiner.Mode.Confine2D;

        var triggerGO = new GameObject("Trigger");
        triggerGO.transform.SetParent(root.transform, false);
        var poly = triggerGO.AddComponent<PolygonCollider2D>();
        poly.isTrigger = true;
        poly.points = new[]
        {
            new Vector2(-5f, -3f), new Vector2(5f, -3f),
            new Vector2(5f, 3f), new Vector2(-5f, 3f)
        };

        var seg = root.AddComponent<CameraSegment>();
        seg.AutoWire();

        return root;
    }
}
#endif