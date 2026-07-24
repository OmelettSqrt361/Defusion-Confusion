using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCaller : MonoBehaviour
{
    public Lock lockObject;

    void AnimEnd()
    {
        lockObject.Unlocked();
    }
}
