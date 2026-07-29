using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerTask : MonoBehaviour
{
    [HideInInspector]
    public Computer headComputer;

    public void DisableOnStartAnim()
    {
        headComputer.CancelOpenComputer();
    }

    public void DisableZoom()
    {
        headComputer.gameObject.GetComponent<Task>().DisableZoomOut();
    }

    public void EnableZoom()
    {
        headComputer.gameObject.GetComponent<Task>().EnableZoomOut();
    }
}
