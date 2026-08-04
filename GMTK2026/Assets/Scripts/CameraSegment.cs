using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSegment : MonoBehaviour
{
    public bool isOn = false;
    List<Collider2D> oldResults = new List<Collider2D>();

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

        CompositeCollider2D collider = GetComponent<CompositeCollider2D>();
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        List<Collider2D> results = new List<Collider2D>();

        collider.OverlapCollider(filter, results);

        isOn = false;
        foreach (Collider2D collision in results)
        {
            if (collision.CompareTag("Player"))
            {
                isOn = true;
                return;
            }
        }
    }

    private void FixedUpdate()
    {
        CompositeCollider2D collider = GetComponent<CompositeCollider2D>();
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        List<Collider2D> results = new List<Collider2D>();

        collider.OverlapCollider(filter, results);
        if (results != oldResults)
        {
            isOn = false;
            foreach (Collider2D collision in results)
            {
                if (collision.CompareTag("Player"))
                {
                    isOn = true;
                    continue;
                }
            }
            oldResults = results;
        }



    }
}
