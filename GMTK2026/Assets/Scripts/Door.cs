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

    public Transform teleportDest;

    AudioSource audioS;
    public AudioClip teleportationSfx;

    public float maxTeleportBuffer;
    float teleportBuffer;


    void Start()
    {
        lockTask = GetComponent<Task>();
        animator = GetComponent<Animator>();
        bScreen = GameObject.FindWithTag("BlackScreen").GetComponent<BlackScreen>();
        audioS = GetComponent<AudioSource>();
        if (!isLocked)
        {
            Unlock();
        }
        lockTask.door = this;
    }

    private void Update()
    {
        if(teleportBuffer > 0)
        {
            teleportBuffer -= Time.deltaTime;
        }
    }

    public void Unlock()
    { 
        lockTask.TurnOff();
        animator.SetBool("Open", true);
        lockTask.taskType = Task.taskTypes.door;
        isLocked = false;
        lockTask.doorTask = false;
    }

    public void Teleport()
    {
        if (!isLocked && teleportBuffer <= 0)
        {
            audioS.PlayOneShot(teleportationSfx);
            bScreen.TurnOn(blackScreenTime);
            lockTask.player.transform.position = teleportDest.position;
            Debug.Log("Teleport");
            teleportBuffer = maxTeleportBuffer;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && !isLocked && teleportBuffer <= 0)
        {
            Teleport();
        }
    }
}
