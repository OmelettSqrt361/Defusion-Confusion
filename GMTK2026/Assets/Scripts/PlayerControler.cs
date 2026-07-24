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
    public Transform handLoc;
    [HideInInspector]
    public GameObject item;
    bool holdingItem;

    
    public string attribute;

    public float maxHoldBuffer; // tiny window, where you can't drop the item
    float holdBuffer;

    // tasks
    [HideInInspector]
    public bool taskNear;
    [HideInInspector]
    public bool doingTask;

    public AudioClip pickup;
    public AudioClip putDown;
    AudioSource audioS;

    GameManager gm;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioS = GetComponent<AudioSource>();
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();

    }

    // Update is called once per frame
    void Update()
    {
        // input management
        vertical = Input.GetAxisRaw("Vertical") * Time.deltaTime;
        horizontal = Input.GetAxisRaw("Horizontal") * Time.deltaTime;

        // movement logic
        moveDir = new Vector2 (horizontal, vertical);
        moveDir.Normalize();

        if (!doingTask)
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

        if(holdingItem)
        {
            // while holding an item
            item.transform.position = handLoc.position;
            if (Input.GetKeyDown(KeyCode.X) && holdBuffer <= 0 && !taskNear)
            {
                ItemDrop();
            }
        }
        else
        {
            // while not holding an item
            if (Input.GetKeyDown(KeyCode.X) && itemsNear.Count != 0 && !taskNear)
            {
                ItemPickup();
            }
        }
    }

    void FixedUpdate()
    {
        // movement logic
        if (!doingTask)
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


    // manage proximity grabbing
    public void ItemAddProximity(GameObject newItem)
    {
        itemsNear.Add(newItem);
    }

    public void ItemCloseProximity(GameObject newItem)
    {
        if (itemsNear.Contains(newItem))
        {
            itemsNear.Remove(newItem);
        }
    }
}
