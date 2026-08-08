using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

// Owns the "which camera is active" state machine. The segment list is now
// derived from the hierarchy (any CameraSegment under this object) rather
// than a manually-populated inspector list - add/remove a segment GameObject
// and this picks it up automatically, no registration step needed.
[ExecuteAlways]
public class CameraSegmentator : MonoBehaviour
{
    public CinemachineVirtualCamera activeCam;
    public float minChange;
    public float priorChange;
    public float camEdgePercent = 1f;

    [SerializeField, Tooltip("Auto-populated from children in the hierarchy - do not edit directly, use Refresh Segments instead.")]
    List<CameraSegment> segments = new List<CameraSegment>();
    public IReadOnlyList<CameraSegment> Segments => segments;

    CameraSegment activeSeg;
    Transform player;
    CameraSegment minSeg;
    CinemachineVirtualCamera minCam;
    float minDist;

    void Awake()
    {
        RefreshSegments();
        FindPlayer();
        SyncActiveSeg();
    }

    void OnEnable()
    {
        RefreshSegments();
        FindPlayer();
        SyncActiveSeg();
    }

    void OnValidate()
    {
        RefreshSegments();
    }

    void FindPlayer()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        player = playerGO != null ? playerGO.transform : null;
    }

    void SyncActiveSeg()
    {
        if (activeCam != null)
            activeSeg = activeCam.gameObject.GetComponentInParent<CameraSegment>();
    }

    // Rebuilds the segment list from the current hierarchy and makes sure
    // every segment has finished wiring its confiner/follow target. Call this
    // manually (e.g. from the "Refresh Segments" button in the inspector)
    // after adding/removing/reparenting segments at runtime or in the editor.
    public void RefreshSegments()
    {
        segments.Clear();
        GetComponentsInChildren(true, segments);
        foreach (var seg in segments)
            seg.AutoWire();
    }

    void FixedUpdate()
    {
        if (activeCam == null || player == null || activeSeg == null) return;

        OptimalSegment();

        Vector3 lookaheadOffset = activeCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset;
        Vector3 lookAt = activeCam.Follow.position + lookaheadOffset;

        float activDist = Dist(activeCam.State.FinalPosition, lookAt, activeSeg.CamPull * activeCam.m_Lens.OrthographicSize);
        if (!IsInCameraView(activeCam, player)) activDist = float.MaxValue;

        if (minSeg == null)
        {
            Debug.Log("Fallback");
        }
        else if (minSeg.Priority > activeSeg.Priority && minDist <= (activDist - priorChange))
        {
            ChangeActiveCam(minCam);
        }
        else if (minDist <= (activDist - minChange))
        {
            ChangeActiveCam(minCam);
        }

        if (!activeSeg.isOn)
        {
            OptimalSegment();
            ChangeActiveCam(minCam);
        }
    }

    public void ChangeActiveCam(CinemachineVirtualCamera newCam)
    {
        if (newCam == null || newCam == activeCam) return;
        activeCam.Priority = 0;
        newCam.Priority = 1;
        activeCam = newCam;
        SyncActiveSeg();
    }

    public float Dist(Vector3 pos1, Vector3 pos2, float pull)
    {
        float dist = Mathf.Sqrt(Mathf.Pow(pos1.x - pos2.x, 2) + Mathf.Pow(pos1.y - pos2.y, 2));
        return dist;
    }

    public bool IsInCameraView(CinemachineVirtualCamera vcam, Transform target)
    {
        CameraState state = vcam.State;
        CameraSegment seg = vcam.GetComponentInParent<CameraSegment>();

        float halfHeight = state.Lens.OrthographicSize;
        float halfWidth = halfHeight * state.Lens.Aspect;

        float edgePercent = (seg != null && seg.OverrideCamEdgePercent != -1f) ? seg.OverrideCamEdgePercent : camEdgePercent;
        halfHeight *= edgePercent;
        halfWidth *= edgePercent;

        Vector3 offset = target.position - state.FinalPosition;
        return Mathf.Abs(offset.x) < halfWidth && Mathf.Abs(offset.y) < halfHeight;
    }

    public bool IsInCameraViewNoWeight(CinemachineVirtualCamera vcam, Transform target)
    {
        CameraState state = vcam.State;
        float halfHeight = state.Lens.OrthographicSize;
        float halfWidth = halfHeight * state.Lens.Aspect;
        Vector3 offset = target.position - state.FinalPosition;
        return Mathf.Abs(offset.x) < halfWidth && Mathf.Abs(offset.y) < halfHeight;
    }

    public void OptimalSegment()
    {
        Vector3 lookaheadOffset = activeCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset;
        Vector3 lookAt = activeCam.Follow.position + lookaheadOffset;

        minDist = float.MaxValue;
        minSeg = null;
        minCam = null;

        foreach (var seg in segments)
        {
            if (seg.Camera == null || !seg.isOn) continue;
            float segDist = Dist(seg.Camera.State.FinalPosition, lookAt, seg.CamPull * seg.Camera.m_Lens.OrthographicSize);
            if (!IsInCameraView(seg.Camera, player)) segDist = float.MaxValue;

            if (segDist < minDist)
            {
                minDist = segDist;
                minCam = seg.Camera;
                minSeg = seg;
            }
        }

        // Fallback 1: ignore camEdgePercent weighting, still require isOn.
        if (minSeg == null) FallbackSearch(lookAt, requireIsOn: true);
        // Fallback 2: ignore isOn entirely too.
        if (minSeg == null) FallbackSearch(lookAt, requireIsOn: false);
    }

    void FallbackSearch(Vector3 lookAt, bool requireIsOn)
    {
        foreach (var seg in segments)
        {
            if (seg.Camera == null) continue;
            if (requireIsOn && !seg.isOn) continue;

            float segDist = Dist(seg.Camera.State.FinalPosition, lookAt, seg.CamPull * seg.Camera.m_Lens.OrthographicSize);
            if (!IsInCameraViewNoWeight(seg.Camera, player)) segDist = float.MaxValue;

            if (segDist < minDist)
            {
                minDist = segDist;
                minCam = seg.Camera;
                minSeg = seg;
            }
        }
    }

#if UNITY_EDITOR
    static readonly Color activeColor = new Color(1f, 0.6f, 0f, 1f);
    static readonly Color onColor = new Color(0.2f, 0.85f, 0.4f, 1f);
    static readonly Color offColor = new Color(0.35f, 0.6f, 1f, 0.8f);
    static readonly Color triggerColor = new Color(1f, 0.9f, 0.2f, 0.9f);

    // Draws every child segment's colliders whenever the CameraSegmentator
    // itself is selected, so you don't need to click into each segment to
    // see its bounds/trigger shape.
    void OnDrawGizmosSelected()
    {
        foreach (var seg in GetComponentsInChildren<CameraSegment>(true))
        {
            bool isActiveSeg = Application.isPlaying && activeSeg == seg;
            Gizmos.color = isActiveSeg ? activeColor : (seg.isOn ? onColor : offColor);

            foreach (var box in seg.GetComponents<BoxCollider2D>())
            {
                if (!box.usedByComposite) continue;
                var t = box.transform;
                Gizmos.matrix = Matrix4x4.TRS(t.TransformPoint(box.offset), t.rotation, t.lossyScale);
                Gizmos.DrawWireCube(Vector3.zero, box.size);
            }
            Gizmos.matrix = Matrix4x4.identity;

            if (seg.IsOnBoundary != null)
            {
                Gizmos.color = triggerColor;
                var t = seg.IsOnBoundary.transform;
                for (int p = 0; p < seg.IsOnBoundary.pathCount; p++)
                {
                    var points = seg.IsOnBoundary.GetPath(p);
                    for (int i = 0; i < points.Length; i++)
                    {
                        Vector3 a = t.TransformPoint(points[i]);
                        Vector3 b = t.TransformPoint(points[(i + 1) % points.Length]);
                        Gizmos.DrawLine(a, b);
                    }
                }
            }

            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(seg.transform.position, $"{seg.name}  P{seg.Priority}");
        }
    }
#endif
}