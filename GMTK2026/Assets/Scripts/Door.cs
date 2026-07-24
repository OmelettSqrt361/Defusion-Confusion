using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isLocked;
    public float blackScreenTime;
    BlackScreen bScreen;
    Task lockTask;
    Animator animator;
    bool isNear;

    public Transform teleportDest;


    void Start()
    {
        lockTask = GetComponent<Task>();
        animator = GetComponent<Animator>();
        bScreen = GameObject.FindWithTag("BlackScreen").GetComponent<BlackScreen>();
        if (!isLocked)
        {
            Unlock();
        }
    }

    // Update is called once per frame
    void Update()
    {
        isNear = lockTask.playerNear;
        if (isNear == true && Input.GetKeyDown(KeyCode.X) && !isLocked)
        {
            bScreen.TurnOn(blackScreenTime);
            lockTask.player.transform.position = teleportDest.position;
            Debug.Log("Teleport");
        }
    }

    public void Unlock()
    {
        lockTask.TurnOff();
        lockTask.noninteractable = true;
        animator.SetBool("Open", true);
        isLocked = false;
    }
}
