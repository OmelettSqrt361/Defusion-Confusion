using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSegment : MonoBehaviour
{
    public bool isOn = false;
    public int priority = 0;
    public float camPull = 1f;
    List<Collider2D> oldResults = new List<Collider2D>();
    public float overrideCamEdgePercent = -1f;
    public PolygonCollider2D isOnBoundary;


    private void Start()
    {
        GetComponentInParent<CameraSegmentator>().segments.Add(
            new CameraSegmentator.Segment
            {
                segmentScript = this,
                camera = GetComponentInChildren<CinemachineVirtualCamera>(),
                segmentObject = this.gameObject,
            }
        );

        UpdateIsOn();
    }

    private void FixedUpdate()
    {
        UpdateIsOn();
    }

    void UpdateIsOn()
    {
        Collider2D collider = isOnBoundary != null
            ? (Collider2D)isOnBoundary
            : GetComponent<CompositeCollider2D>();

        if (collider == null)
        {
            Debug.LogWarning($"{name}: CameraSegment has no boundary collider assigned (isOnBoundary is null and no CompositeCollider2D found).", this);
            return;
        }

        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D().NoFilter();

        collider.OverlapCollider(filter, results);

        isOn = false;
        foreach (Collider2D collision in results)
        {
            if (collision.CompareTag("Player"))
            {
                isOn = true;
                break;
            }
        }
    }
}
