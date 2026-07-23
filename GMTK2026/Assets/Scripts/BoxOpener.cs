using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxOpener : MonoBehaviour
{

    public GameObject activate;
    public Animator animator;

    public void OpenBox()
    {
        if (activate != null) { activate.SetActive(true); }
        animator.SetTrigger("Open");
    }
}
