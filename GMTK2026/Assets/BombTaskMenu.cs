using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTaskMenu : MonoBehaviour
{

    Bomb bombTask;

    // Start is called before the first frame update
    void Start()
    {
        bombTask = GetComponent<TaskMenuMain>().controler.gameObject.GetComponent<Bomb>();  
    }

    public void Reset()
    {
        bombTask.ResetTimer();
    }

}
