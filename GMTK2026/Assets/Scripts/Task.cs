using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class Task : MonoBehaviour
{
    public enum taskTypes { bomb, box, door, computer, text, lockAndKey}
    public taskTypes taskType;

    // Object interaction
    public bool closestInteractable = false;
    public GameObject player;
    [SerializeField]  Animator animator;

    // Bomb Things
    [SerializeField] Bomb b;

    // Task menu interaction
    [SerializeField] GameManager gm;
    [SerializeField] CinemachineVirtualCamera mainCam;
    [SerializeField] CinemachineVirtualCamera taskCam;
    [SerializeField] CinemachineVirtualCamera zoomCam;

    [SerializeField] GameObject[] zoomButtons;
    public GameObject taskMenu;
    [SerializeField] bool isRunning = false;
    [SerializeField]  bool isZoomed = false;

    // Tool usage
    public string[] toolNames;
    public GameObject[] toolsToActivate;
    public List<GameObject> activeTools = new List<GameObject>();

    // Deactivation
    public bool noninteractable = false;

    [SerializeField] AudioSource audiosS;
    public bool hasAudio;
    public AudioClip clip;

    [SerializeField] bool noZoomingOut;

    void Start()
    {
        if (taskType == taskTypes.bomb)
        {
            b = this.gameObject.GetComponent<Bomb>();
        }
        mainCam = GameObject.FindWithTag("MainVCamera").GetComponent<CinemachineVirtualCamera>();
        player = GameObject.FindWithTag("Player");
        taskCam = gameObject.GetComponentInChildren<CinemachineVirtualCamera>();
        animator = gameObject.GetComponent<Animator>();
        if (taskMenu != null) { zoomButtons = GetChildrenWithTag(taskMenu, "Zoom"); }
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        if (hasAudio) { audiosS = gameObject.GetComponent<AudioSource>(); }
        noZoomingOut = false;

        activeTools.Clear();
    }


    void Update()
    {
        animator.SetBool("Near", closestInteractable);
        if (isRunning)
        {
            // if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.E)) && !isRunning && !noZoomingOut) { TurnOn(); } else 
            if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.E)) && isZoomed && !noZoomingOut)
            {
                ZoomOut();
            }
            else if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.E)) && !noZoomingOut)
            {
                TurnOff();
            }
        }

        // change size of active tools
        if (toolsToActivate.Length > 0)
        {
            float mainCamSize = Camera.main.orthographicSize;
            float vcamSize = taskCam.m_Lens.OrthographicSize;
            float mainToVRatio = mainCamSize / vcamSize;

            foreach (var tool in activeTools)
            {
                if (tool.activeSelf)
                {
                    tool.transform.localScale = new Vector2(mainToVRatio, mainToVRatio);
                }
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.gameObject;
            // player.GetComponent<PlayerControler>().taskNear = true;
            player.GetComponent<PlayerControler>().TaskAddProximity(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // player.GetComponent<PlayerControler>().taskNear = false;
            player.GetComponent<PlayerControler>().TaskCloseProximity(gameObject);
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

        if(hasAudio) { audiosS.PlayOneShot(clip); }

        // tool handling
        for (int i = 0; i < toolNames.Length; i++) 
        { 
            if(toolNames[i] == player.GetComponent<PlayerControler>().attribute)
            {
                Debug.Log($"Spawning: {toolNames[i]}");
                toolsToActivate[i].SetActive(true);
                activeTools.Add(toolsToActivate[i]);
            }
        }

    }

    public void TurnOff()
    {
        isRunning = false;
        if (player != null && taskType != taskTypes.door)
        {
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

    public void DisableZoomOut()
    {
        noZoomingOut = true;
    }

    public void EnableZoomOut()
    {
        noZoomingOut = false;
    }

    public void Defuse()
    {
        if (b != null) { b.Defuse(true); }
    }
}
