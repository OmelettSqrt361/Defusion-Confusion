using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControler : MonoBehaviour
{


    // movement
    Rigidbody2D rb;
    public float runVelocity; // sets the max speed
    float velocity; // current velocity
    public float runVelocityIncrease; // for adding velocity when time is low

    float horizontal;
    float vertical;

    float windup; // current windup
    public float windupTime; // tiny window to not start running immediately
    Vector2 moveDir;

    //animation
    Animator animator;

    // items
    [HideInInspector]
    public List<GameObject> itemsNear = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> tasksNear = new List<GameObject>();
    public Transform handLoc;
    [HideInInspector]
    public GameObject item;
    bool holdingItem;

    
    // items
    public string attribute;
    public float maxHoldBuffer; // tiny window, where you can't drop the item
    float holdBuffer;

    // tasks
    [SerializeField] public bool doingTask;

    // audio
    public AudioClip pickup;
    public AudioClip putDown;
    AudioSource audioS;

    // gameEnd
    [HideInInspector]
    public bool hasEnded = false;
    [HideInInspector]
    public bool notBegun = true;
    GameManager gm;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioS = GetComponent<AudioSource>();
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
    }

    void Update()
    {
        // input management
        vertical = Input.GetAxisRaw("Vertical") * Time.deltaTime;
        horizontal = Input.GetAxisRaw("Horizontal") * Time.deltaTime;

        // movement logic
        moveDir = new Vector2 (horizontal, vertical);
        moveDir.Normalize();

        if (!doingTask && !hasEnded && !notBegun)
        {
            animator.SetFloat("Velocity", velocity); // animation player
            if (horizontal == 0 && vertical == 0)
            {
                windup = windupTime;
                velocity = 0;
            }
            else if (windup > 0)
            {
                windup = windup - Time.deltaTime;
                velocity = (runVelocity + (runVelocityIncrease * gm.currentBombFactor)) * ((windupTime - windup) / windupTime);
            }
            moveDir = moveDir * velocity;
        }

        // item management

        if (holdBuffer > 0)
        {
            holdBuffer -= Time.deltaTime;
        }

        SetClosestInteractable();

        Debug.Log($"Doing task: {doingTask}");
        if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.E)) && (doingTask == false))
        {
            Debug.Log("Searching!");
            if(tasksNear.Count > 0)
            {
                if (holdingItem)
                {
                    EventInteract();
                }
                else
                {
                    ItemEventInteract();
                }
            }
            else if(itemsNear.Count > 0)
            {
                // Interact with Items
                if (holdingItem)
                {
                    if(holdBuffer <= 0)
                    {
                        ItemDrop();
                    }
                    else
                    {
                        // do nothing, until holdbuffer is empty
                    }
                }
                else
                {
                    ItemPickup();
                }
            }
            else if (holdingItem && holdBuffer <= 0)
            {
                ItemDrop();
            }
        }
        else if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.E)))
        {
            Debug.Log("Doing Task");
        }

        /*  --------------------
         *  OLD INTERACTION CODE
         *  --------------------
        if(holdingItem)
        {
            // while holding an item
            item.transform.position = handLoc.position;
            if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.E)) && holdBuffer <= 0 && !taskNear)
            {
                ItemDrop();
            }
        }
        else
        {
            // while not holding an item
            if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.E)) && itemsNear.Count != 0 && !taskNear)
            {
                ItemPickup();
            }
        }
        */
    }

    void FixedUpdate()
    {
        // movement logic
        if (!doingTask && !hasEnded && !notBegun)
        {
            rb.velocity = moveDir;
        } else
        {
            animator.SetFloat("Velocity", 0);
            rb.velocity = new Vector2(0, 0);
        }
    }

    public void ItemDrop()
    {
        Debug.Log("Dropped the item");
        item.GetComponent<Item>().ItemDropped();
        attribute = "";

        audioS.PlayOneShot(putDown);
        item = null;
        holdingItem = false;
    }

    public void ItemPickup()
    {
        GameObject nearestItem = null;
        float leastDistance = float.MaxValue;

        foreach (var i in itemsNear)
        {
            // distance calculation
            float dist = Mathf.Sqrt(
                Mathf.Pow(transform.position.x - i.transform.position.x,2) 
                + Mathf.Pow(transform.position.y - i.transform.position.y, 2));
            if(dist < leastDistance)
            {
                leastDistance = dist;
                nearestItem = i;
            }
        }

        // set item as held
        item = nearestItem;
        attribute = item.GetComponent<Item>().attribute;
        item.GetComponent<Item>().ItemGrabbed();

        audioS.PlayOneShot(pickup);

        holdingItem = true;
        holdBuffer = maxHoldBuffer;
    }

    public void EventInteract()
    {
        GameObject nearestItem = null;
        float leastDistance = float.MaxValue;

        foreach (var i in tasksNear)
        {
            // distance calculation
            float dist = Mathf.Sqrt(
                Mathf.Pow(transform.position.x - i.transform.position.x, 2)
                + Mathf.Pow(transform.position.y - i.transform.position.y, 2));
            if (dist < leastDistance)
            {
                leastDistance = dist;
                nearestItem = i;
            }
        }

        // Open Near Tasks
        nearestItem.GetComponent<Task>().TurnOn();
        doingTask = true;
    }

    public void ItemEventInteract()
    {
        GameObject nearestItem = null;
        float leastDistance = float.MaxValue;

        // Check items
        foreach (var i in itemsNear)
        {
            // distance calculation
            float dist = Mathf.Sqrt(
                Mathf.Pow(transform.position.x - i.transform.position.x, 2)
                + Mathf.Pow(transform.position.y - i.transform.position.y, 2));
            if (dist < leastDistance)
            {
                leastDistance = dist;
                nearestItem = i;
            }
        }

        // Check Tasks
        foreach (var i in tasksNear)
        {
            // distance calculation
            float dist = Mathf.Sqrt(
                Mathf.Pow(transform.position.x - i.transform.position.x, 2)
                + Mathf.Pow(transform.position.y - i.transform.position.y, 2));
            if (dist < leastDistance)
            {
                leastDistance = dist;
                nearestItem = i;
            }
        }

        if (itemsNear.Contains(nearestItem)) // If it is an item just treat it as such
        {
            item = nearestItem;
            attribute = item.GetComponent<Item>().attribute;
            item.GetComponent<Item>().ItemGrabbed();

            audioS.PlayOneShot(pickup);

            holdingItem = true;
            holdBuffer = maxHoldBuffer;
        }
        else // If it's a task turn it on
        {
            nearestItem.GetComponent<Task>().TurnOn();
            doingTask = true;
        }
    }

    public void SetClosestInteractable()
    {
        GameObject nearestItem = null;
        float leastDistance = float.MaxValue;

        // Check items
        if (!holdingItem)
        {
            foreach (var i in itemsNear)
            {
                // distance calculation
                float dist = Mathf.Sqrt(
                    Mathf.Pow(transform.position.x - i.transform.position.x, 2)
                    + Mathf.Pow(transform.position.y - i.transform.position.y, 2));
                if (dist < leastDistance)
                {
                    leastDistance = dist;
                    nearestItem = i;
                }
            }
        }

        // Check Tasks
        foreach (var i in tasksNear)
        {
            if (!i.GetComponent<Task>().noninteractable)
            {
                // distance calculation
                float dist = Mathf.Sqrt(
                    Mathf.Pow(transform.position.x - i.transform.position.x, 2)
                    + Mathf.Pow(transform.position.y - i.transform.position.y, 2));
                if (dist < leastDistance)
                {
                    leastDistance = dist;
                    nearestItem = i;
                }
            }
        }

        if (nearestItem != null)
        {
            if (itemsNear.Contains(nearestItem)) // Set Item as closest
            {
                nearestItem.GetComponent<Item>().closestInteractable = true;
            }
            else // Set Task as closest
            {
                nearestItem.GetComponent<Task>().closestInteractable = true;
            }
        }

        // Set all other interactables as not closest
        foreach (var i in itemsNear)
        {
            if(i != nearestItem)
            {
                i.GetComponent<Item>().closestInteractable = false;
            }
        }
        foreach (var i in tasksNear)
        {
            if (i != nearestItem)
            {
                i.GetComponent<Task>().closestInteractable = false;
            }
        }
    }

    // manage proximity grabbing
    public void ItemAddProximity(GameObject newItem)
    {
        itemsNear.Add(newItem);
    }

    public void TaskAddProximity(GameObject newItem)
    {
        tasksNear.Add(newItem);
    }

    public void ItemCloseProximity(GameObject newItem)
    {
        if (itemsNear.Contains(newItem))
        {
            itemsNear.Remove(newItem);
        }
        newItem.GetComponent<Item>().closestInteractable = false;
    }

    public void TaskCloseProximity(GameObject newItem)
    {
        if (tasksNear.Contains(newItem))
        {
            tasksNear.Remove(newItem);
        }
        newItem.GetComponent<Task>().closestInteractable = false;
    }
}
