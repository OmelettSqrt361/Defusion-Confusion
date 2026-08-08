using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CameraSegmentator;

public class CameraSegmentator : MonoBehaviour
{
    [System.Serializable]
    public struct Segment
    {
        public CinemachineVirtualCamera camera;
        public CameraSegment segmentScript;
        public GameObject segmentObject;

        public Segment(CameraSegment cseg, CinemachineVirtualCamera cam, GameObject sego, Transform coord)
        {
            this.segmentScript = cseg;
            this.camera = cam;
            this.segmentObject = sego;
        }
    }

    [SerializeField] public List<Segment> segments = new List<Segment>();
    CameraSegment activeSeg;
    public CinemachineVirtualCamera activeCam;
    public float minChange;
    public float priorChange;
    Transform player;
    CameraSegment minSeg = null;
    CinemachineVirtualCamera minCam;
    float minDist;


    public float camEdgePercent = 1f; // if the player is not in bounding box given by camEdgePrecent * the bounds of the camera, then it is treated as not there
                                 // it's kind of a dead zone


    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        activeSeg = activeCam.gameObject.GetComponentInParent<CameraSegment>();
        OptimalSegment();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // calculate closest camera
        OptimalSegment();

        Vector3 lookaheadOffset = activeCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset;
        Vector3 lookAt = activeCam.Follow.position + lookaheadOffset;

        // check if threshhold has been surpassed
        float activDist = Dist(activeCam.State.FinalPosition, lookAt, activeSeg.camPull * activeCam.m_Lens.OrthographicSize);
        if (!IsInCameraView(activeCam, player.transform)) { activDist = float.MaxValue; }
        if (minSeg == null)
        {
            Debug.Log("Fallback");
        }
        else if (minSeg.priority > activeSeg.priority && minDist <= (activDist - priorChange))
        {
            ChangeActiveCam(minCam);
        }
        else if (minDist <= (activDist - minChange))
        {
            ChangeActiveCam(minCam);
        }

        if(!activeSeg.isOn)
        {
            OptimalSegment();
            ChangeActiveCam(minCam);
        }
    }

    public void ChangeActiveCam(CinemachineVirtualCamera newCam)
    {
        activeCam.Priority = 0;
        newCam.Priority = 1;
        activeCam = newCam;
        activeSeg = activeCam.gameObject.GetComponentInParent<CameraSegment>();
    }

    public float Dist(Vector3 pos1, Vector3 pos2, float pull)
    {
        float dist = Mathf.Sqrt(Mathf.Pow(pos1.x - pos2.x, 2) + Mathf.Pow(pos1.y - pos2.y, 2));
        float weightedDist = dist * dist / pull;
        return dist;
    }

    public bool IsInCameraView(CinemachineVirtualCamera vcam, Transform target)
    {
        CameraState state = vcam.State;
        CameraSegment seg = vcam.GetComponentInParent<CameraSegment>();

        float halfHeight = state.Lens.OrthographicSize;
        float halfWidth = halfHeight * state.Lens.Aspect;

        if(seg.overrideCamEdgePercent != -1f)
        {
            halfHeight *= seg.overrideCamEdgePercent;
            halfWidth *= seg.overrideCamEdgePercent;
        }
        else
        {
            halfHeight *= camEdgePercent;
            halfWidth *= camEdgePercent;
        }

        Vector3 offset = target.position - state.FinalPosition;

        return Mathf.Abs(offset.x) < halfWidth && Mathf.Abs(offset.y) < halfHeight;
    }

    public bool IsInCameraViewNoWeight(CinemachineVirtualCamera vcam, Transform target)
    {
        CameraState state = vcam.State;
        CameraSegment seg = vcam.GetComponentInParent<CameraSegment>();

        float halfHeight = state.Lens.OrthographicSize;
        float halfWidth = halfHeight * state.Lens.Aspect;

        Vector3 offset = target.position - state.FinalPosition;

        return Mathf.Abs(offset.x) < halfWidth && Mathf.Abs(offset.y) < halfHeight;
    }

    public void OptimalSegment()
    {
        // calculate closest camera
        Vector3 lookAt;
        Vector3 lookaheadOffset = activeCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset;
        lookAt = activeCam.Follow.position + lookaheadOffset;

        minDist = float.MaxValue;   // use class field, don't redeclare
        minSeg = null;
        minCam = null;               // use class field, don't redeclare

        foreach (Segment segment in segments)
        {
            if (segment.segmentScript.isOn)
            {
                float segDist = Dist(segment.camera.State.FinalPosition, lookAt, segment.segmentScript.camPull * segment.camera.m_Lens.OrthographicSize);
                if (!IsInCameraView(segment.camera, player.transform)) { segDist = float.MaxValue; }

                if (segDist < minDist)
                {
                    minDist = segDist;
                    minCam = segment.camera;
                    minSeg = segment.segmentScript;
                }
            }
        }

        // fallback
        if (minSeg == null)
        {
            foreach (Segment segment in segments)
            {
                if (segment.segmentScript.isOn)
                {
                    float segDist = Dist(segment.camera.State.FinalPosition, lookAt, segment.segmentScript.camPull * segment.camera.m_Lens.OrthographicSize);
                    if (!IsInCameraViewNoWeight(segment.camera, player.transform)) { segDist = float.MaxValue; }

                    if (segDist < minDist)
                    {
                        minDist = segDist;
                        minCam = segment.camera;
                        minSeg = segment.segmentScript;
                    }
                }
            }
        }

        // fallback 2
        if (minSeg == null)
        {
            foreach (Segment segment in segments)
            {
                float segDist = Dist(segment.camera.State.FinalPosition, lookAt, segment.segmentScript.camPull * segment.camera.m_Lens.OrthographicSize);
                if (!IsInCameraViewNoWeight(segment.camera, player.transform)) { segDist = float.MaxValue; }

                if (segDist < minDist)
                {
                    minDist = segDist;
                    minCam = segment.camera;
                    minSeg = segment.segmentScript;
                }
            }
        }
    }
}
