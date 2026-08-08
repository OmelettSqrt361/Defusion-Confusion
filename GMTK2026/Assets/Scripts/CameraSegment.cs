using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

// A single rectangular (or composite-shaped) area of the level.
// Owns: the colliders that define its camera bounds, an optional dedicated
// trigger boundary that turns it on/off, and the Cinemachine vcam that frames it.
//
// This script is now self-configuring: as long as it has a CompositeCollider2D
// on itself and a CinemachineVirtualCamera somewhere in its children, OnValidate/
// OnEnable will wire the confiner bounds and follow target automatically. You no
// longer need to manually drag references in the inspector for each new segment.
[ExecuteAlways]
[DisallowMultipleComponent]
public class CameraSegment : MonoBehaviour
{
    [Header("Behavior")]
    [Tooltip("Higher priority segments take over the active camera even from farther away (see CameraSegmentator.priorChange).")]
    public int priority = 0;
    [Tooltip("Higher pull makes this segment 'win' the active camera from farther away.")]
    public float camPull = 1f;
    [Tooltip("-1 = use CameraSegmentator's global camEdgePercent instead.")]
    public float overrideCamEdgePercent = -1f;

    [Header("Optional shared config (retune many segments from one asset)")]
    public CameraSegmentPreset preset;

    [Header("References (auto-filled - leave empty)")]
    [SerializeField] CinemachineVirtualCamera vcam;
    [SerializeField] CompositeCollider2D boundsCollider;
    [SerializeField] PolygonCollider2D isOnBoundary;

    public bool isOn { get; private set; }
    public CinemachineVirtualCamera Camera => vcam;
    public CompositeCollider2D BoundsCollider => boundsCollider;
    public PolygonCollider2D IsOnBoundary => isOnBoundary;

    // Effective values: preset provides defaults, local fields override when non-default.
    public int Priority => preset != null && priority == 0 ? preset.priority : priority;
    public float CamPull => preset != null && camPull == 1f ? preset.camPull : camPull;
    public float OverrideCamEdgePercent =>
        overrideCamEdgePercent != -1f ? overrideCamEdgePercent :
        (preset != null ? preset.overrideCamEdgePercent : -1f);

    static readonly List<Collider2D> overlapResults = new List<Collider2D>();
    static readonly ContactFilter2D noFilter = new ContactFilter2D().NoFilter();

    void OnEnable() => AutoWire();
    void OnValidate() => AutoWire();
    void Start() => UpdateIsOn();
    void FixedUpdate() => UpdateIsOn();

    // Finds/creates the pieces this segment needs and wires them together.
    // Safe to call repeatedly - it's idempotent.
    public void AutoWire()
    {
        if (vcam == null) vcam = GetComponentInChildren<CinemachineVirtualCamera>(true);
        if (boundsCollider == null) boundsCollider = GetComponent<CompositeCollider2D>();

        if (isOnBoundary == null)
        {
            // Prefer a dedicated trigger collider (isTrigger, not part of the composite)
            // over falling back to the composite bounds itself.
            foreach (var p in GetComponentsInChildren<PolygonCollider2D>(true))
            {
                if (p.isTrigger && !p.usedByComposite) { isOnBoundary = p; break; }
            }
        }

        if (vcam == null) return;

        var confiner = vcam.GetComponent<CinemachineConfiner>();
        if (confiner == null) confiner = vcam.gameObject.AddComponent<CinemachineConfiner>();
        confiner.m_ConfineMode = CinemachineConfiner.Mode.Confine2D;
        if (boundsCollider != null && confiner.m_BoundingShape2D != boundsCollider)
            confiner.m_BoundingShape2D = boundsCollider;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && vcam.m_Follow != player.transform)
            vcam.m_Follow = player.transform;
    }

    void UpdateIsOn()
    {
        Collider2D collider = isOnBoundary != null ? (Collider2D)isOnBoundary : boundsCollider;
        if (collider == null)
        {
            if (Application.isPlaying)
                Debug.LogWarning($"{name}: CameraSegment has no boundary collider assigned.", this);
            return;
        }

        overlapResults.Clear();
        collider.OverlapCollider(noFilter, overlapResults);

        isOn = false;
        foreach (var c in overlapResults)
        {
            if (c.CompareTag("Player")) { isOn = true; break; }
        }
    }
}