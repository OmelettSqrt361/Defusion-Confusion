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
    public bool hasTaskMenu;
    public GameObject taskMenuPrefab;
    public GameObject taskMenu;
    [SerializeField] bool isRunning = false;
    [SerializeField] bool isZoomed = false;

    // Tool usage
    public string[] toolNames;
    public GameObject[] toolsToActivate;
    public List<GameObject> activeTools = new List<GameObject>();

    // Deactivation
    public bool noninteractable = false;
    public bool doorTask = false;

    [SerializeField] AudioSource audiosS;
    public bool hasAudio;
    public AudioClip clip;

    [SerializeField] bool noZoomingOut;

    public Door door;
    public GameObject taskCanvas;

    void Start()
    {
        // get all necessary components
        mainCam = GameObject.FindWithTag("MainVCamera").GetComponent<CinemachineVirtualCamera>();
        player = GameObject.FindWithTag("Player");
        taskCam = gameObject.GetComponentInChildren<CinemachineVirtualCamera>();
        animator = gameObject.GetComponent<Animator>();
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        taskCanvas = GameObject.FindWithTag("Task Canvas");

        // create a task menu
        if (hasTaskMenu)
        {
            taskMenu = Instantiate(taskMenuPrefab, new Vector3(taskCam.gameObject.transform.position.x, taskCam.gameObject.transform.position.y, 0), Quaternion.identity, taskCanvas.transform);
            taskMenu.GetComponent<TaskMenuMain>().controler = this;
            toolsToActivate = GetMatchingChildren(taskMenu, toolNames);
            zoomButtons = GetChildrenWithTag(taskMenu, "Zoom");
        }

        // handle audio
        if (hasAudio) { audiosS = gameObject.GetComponent<AudioSource>(); }
        // handle doors
        if (taskType == taskTypes.door) { door = gameObject.GetComponent<Door>(); }
        // handle bombs
        if (taskType == taskTypes.bomb) { b = this.gameObject.GetComponent<Bomb>(); }

        // initial variables
        noZoomingOut = false;
        activeTools.Clear();
    }


    void Update()
    {
        animator.SetBool("Near", closestInteractable);
        if (isRunning)
        {
            if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.E)) && isZoomed && !noZoomingOut)
            {
                ZoomOut();
            }
            else if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.E)) && !noZoomingOut && player.GetComponent<PlayerControler>().taskStartBuffer <= 0)
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
                if (tool.activeSelf && tool.tag != "TaskMessage")
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
        if(taskType != taskTypes.door || doorTask == true)
        {
            isRunning = true;
            taskMenu.SetActive(true);
            player.GetComponent<PlayerControler>().doingTask = true;
            taskCam.Priority = 1;
            mainCam.Priority = 0;
            gm.HideOverlayMenu();

            Debug.Log($"TurningOn: {gameObject.name}");

            if (hasAudio) { audiosS.PlayOneShot(clip); }

            // tool handling
            for (int i = 0; i < toolNames.Length; i++)
            {
                if (toolNames[i] == player.GetComponent<PlayerControler>().attribute)
                {
                    Debug.Log($"Spawning: {toolNames[i]}");
                    toolsToActivate[i].SetActive(true);
                    activeTools.Add(toolsToActivate[i]);
                }
            }
        } else if (taskType == taskTypes.door) // when door task is done, just teleport
        {
            door.Teleport();
        }

    }

    public void TurnOff()
    {
        isRunning = false;
        if (player != null && taskType != taskTypes.door)
        {
            Debug.Log($"TurningOff: {gameObject.name}");

            taskMenu.SetActive(false);
            PlayerControler pc = player.GetComponent<PlayerControler>();
            pc.doingTask = false;
            pc.TaskEnd();
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

    public GameObject[] GetMatchingChildren(GameObject targetParent, string[] targetNames)
    {
        // Safety check: if inputs are invalid, return null
        if (targetParent == null || targetNames == null)
        {
            Debug.LogWarning("Target Parent or targetNames array is null.");
            return null;
        }

        // Initialize array with the same length as targetNames (defaults to null for all elements)
        GameObject[] matchedObjects = new GameObject[targetNames.Length];

        // Loop through direct children only
        foreach (Transform child in targetParent.transform)
        {
            for (int i = 0; i < targetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(targetNames[i]))
                    continue;

                // Case-insensitive match
                if (child.name.Equals(targetNames[i], System.StringComparison.OrdinalIgnoreCase))
                {
                    matchedObjects[i] = child.gameObject;
                }
            }
        }

        return matchedObjects;
    }
}
