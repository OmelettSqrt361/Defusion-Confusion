using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCaller : MonoBehaviour
{
    public Lock lockObject;
    public Task lockTask;

    void AnimEnd()
    {
        lockObject.Unlocked();
    }

    public void NoZoomOut()
    {
        lockTask.DisableZoomOut();
    }

    public void ZoomOutOkay()
    {
        lockTask.EnableZoomOut();
    }

}
