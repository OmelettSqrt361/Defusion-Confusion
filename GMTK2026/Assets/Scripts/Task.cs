using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Task : MonoBehaviour
{
    // Object interaction
    bool playerNear = false;
    GameObject player;
    Animator animator;

    // Bomb Things
    public bool isBomb;
    Bomb b;

    // Task menu interaction
    GameManager gm;
    CinemachineVirtualCamera mainCam;
    CinemachineVirtualCamera taskCam;
    CinemachineVirtualCamera zoomCam;


    GameObject[] zoomButtons;
    public GameObject taskMenu;
    bool isRunning = false;
    bool isZoomed = false;

    // Tool usage
    public string[] toolNames;
    public GameObject[] toolsToActivate;

    void Start()
    {
        if (isBomb)
        {
            b = this.gameObject.GetComponent<Bomb>();
        }
        mainCam = GameObject.FindWithTag("MainVCamera").GetComponent<CinemachineVirtualCamera>();
        taskCam = gameObject.GetComponentInChildren<CinemachineVirtualCamera>();
        animator = gameObject.GetComponent<Animator>();
        zoomButtons = GetChildrenWithTag(taskMenu, "Zoom");
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
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

        // tool handling
        for (int i = 0; i < toolNames.Length; i++) 
        { 
            if(toolNames[i] == player.GetComponent<PlayerControler>().attribute)
            {
                Debug.Log(toolNames[i]);
                toolsToActivate[i].SetActive(true);
            }
        }

    }

    public void TurnOff()
    {
        isRunning = false;
        taskMenu.SetActive(false);
        player.GetComponent<PlayerControler>().doingTask = false;
        taskCam.Priority = 0;
        mainCam.Priority = 1;
        gm.ShowOverlayMenu();

        // hide tools
        foreach (var tool in toolsToActivate)
        {
            tool.SetActive(false);
        }

    }

    public void ZoomIn(CinemachineVirtualCamera newCam)
    {
        newCam.Priority = 1;
        taskCam.Priority = 0;
        zoomCam = newCam;
        isZoomed = true;

        foreach (var button in zoomButtons)
        {
            button.SetActive(false);
        }
    }

    public void ZoomOut()
    {
        zoomCam.Priority = 0;
        taskCam.Priority = 1;
        isZoomed = false;

        foreach (var button in zoomButtons)
        {
            button.SetActive(true);
        }
    }

    // Helper functions
    GameObject[] GetChildrenWithTag(GameObject parent, string tag)
    {
        List<GameObject> matchingChildren = new List<GameObject>();
        Transform[] allChildren = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag(tag))
            {
                matchingChildren.Add(child.gameObject);
            }
        }
        return matchingChildren.ToArray();
    }
}
