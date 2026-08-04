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
    public CinemachineVirtualCamera activeCam;
    public float minChange;
    Transform player;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 lookaheadOffset = activeCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset;
        Vector3 predictedTargetPos = activeCam.Follow.position + lookaheadOffset;

        // calculate closest camera
        float minDist = float.MaxValue;
        CinemachineVirtualCamera minCam = null;
        foreach (Segment segment in segments) {
            float segDist = Dist(segment.camera.State.FinalPosition, predictedTargetPos) / segment.camera.m_Lens.OrthographicSize;

            if (segDist < minDist)
            {
                minDist = segDist;
                minCam = segment.camera;
            }
        }

        // check if threshhold has been surpassed
        float activDist = Dist(activeCam.State.FinalPosition, predictedTargetPos) / activeCam.m_Lens.OrthographicSize;
        if (minDist <= activDist - minChange)
        {
            if (minCam == null)
            {
                Debug.Log("Fallback");
            }
            else if (minCam != activeCam)
            {
                ChangeActiveCam(minCam);
            }
        }
    }

    public void ChangeActiveCam(CinemachineVirtualCamera newCam)
    {
        activeCam.Priority = 0;
        newCam.Priority = 1;
        activeCam = newCam;
    }

    public float Dist(Vector3 pos1, Vector3 pos2)
    {
        float dist = Mathf.Sqrt(Mathf.Pow(pos1.x - pos2.x, 2) + Mathf.Pow(pos1.y - pos2.y, 2));
        return dist;
    }
}
