using UnityEngine;

// Optional: group segments into categories (e.g. "Indoor Room", "Corridor",
// "Boss Arena") so retuning pull/priority for a whole category is one edit
// instead of clicking into every segment that belongs to it.
//
// A CameraSegment only falls back to these values when its own local
// priority/camPull are left at their defaults (0 and 1) - set the field
// directly on the segment to override the preset for that one instance.
[CreateAssetMenu(menuName = "Camera System/Segment Preset", fileName = "New Segment Preset")]
public class CameraSegmentPreset : ScriptableObject
{
    public int priority = 0;
    public float camPull = 1f;
    public float overrideCamEdgePercent = -1f;
}
