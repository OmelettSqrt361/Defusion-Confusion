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

    bool done;

    void Start()
    {
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
}
