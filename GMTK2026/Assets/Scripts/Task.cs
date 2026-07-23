using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Task : MonoBehaviour
{
    // Object interaction
    bool playerNear = false;
    GameObject player;
    public Animator animator;

    // Bomb Things
    public bool isBomb;
    Bomb b;

    // Task menu interaction
    public GameManager gm;

    public CinemachineVirtualCamera mainCam;
    public CinemachineVirtualCamera taskCam;
    CinemachineVirtualCamera zoomCam;
    public GameObject taskMenu;
    bool isRunning = false;
    bool isZoomed = false;

    void Start()
    {
        if (isBomb)
        {
            b = this.gameObject.GetComponent<Bomb>();
        }
    }


    void Update()
    {
        animator.SetBool("Near", playerNear);
        if (playerNear)
        {
            if (Input.GetKeyDown(KeyCode.X) && !isRunning)
            {
                TurnOn();
            }
            else if (Input.GetKeyDown(KeyCode.X) && isZoomed)
            {
                ZoomOut();
            }
            else if (Input.GetKeyDown(KeyCode.X) && isRunning)
            {
                TurnOff();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNear = true;
            player = collision.gameObject;
            player.GetComponent<PlayerControler>().taskNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNear = false;
            player.GetComponent<PlayerControler>().taskNear = false;
        }
    }

    public void TurnOn()
    {
        isRunning = true;
        taskMenu.SetActive(true);
        player.GetComponent<PlayerControler>().doingTask = true;
        taskCam.Priority = 1;
        mainCam.Priority = 0;
        gm.HideOverlayMenu();
    }

    public void TurnOff()
    {
        isRunning = false;
        taskMenu.SetActive(false);
        player.GetComponent<PlayerControler>().doingTask = false;
        taskCam.Priority = 0;
        mainCam.Priority = 1;
        gm.ShowOverlayMenu();
    }

    public void ZoomIn(CinemachineVirtualCamera newCam)
    {
        newCam.Priority = 1;
        taskCam.Priority = 0;
        zoomCam = newCam;
        isZoomed = true;
    }

    public void ZoomOut()
    {
        zoomCam.Priority = 0;
        taskCam.Priority = 1;
        isZoomed = false;
    }
}
