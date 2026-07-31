using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskMenuMain : MonoBehaviour
{
    // This is just an acessPoint script that is common between all the tasks
    // This controler is set by the Task script so the subscripts for the task may have the reference for it!
    public Task controler;

    public void ZoomIn(CinemachineVirtualCamera newCam)
    {
        Debug.Log($"{newCam.name}");
        controler.ZoomIn(newCam);
    }
}
