using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneHopper : MonoBehaviour
{
    public void Hop(int nextSceneID)
    {
        SceneManager.LoadScene(nextSceneID);
    }
}
