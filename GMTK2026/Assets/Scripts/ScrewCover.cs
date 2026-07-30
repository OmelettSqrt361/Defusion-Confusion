using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ScrewCover : MonoBehaviour
{

    public int numberOfScrews;
    [HideInInspector]
    public int screwsUnsrewed;
    Animator animator;
    Task task;

    bool done;

    void Start()
    {
        task = gameObject.GetComponentInParent<TaskMenuMain>().controler;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(screwsUnsrewed == numberOfScrews & !done)
        {
            TakeOff();
        }
    }

    void TakeOff()
    {
        animator.SetTrigger("Open");
        animator.SetBool("Done", true);
        done = true;
    }

    public void SetZoomOut(int zoomOut)
    {
        if (zoomOut == 0)
        {
            task.DisableZoomOut();
        }
        else
        {
            task.EnableZoomOut();
        }
    }
}
