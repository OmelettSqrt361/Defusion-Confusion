using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControler : MonoBehaviour
{


    // movement
    public Rigidbody2D rb;
    public float runVelocity;
    float velocity;
    float horizontal;
    float vertical;

    float windup;
    public float windupTime;
    Vector2 moveDir;

    //animation
    public Animator animator;


    // items
    public List<GameObject> itemsNear = new List<GameObject>();
    public Transform handLoc;
    public GameObject item;
    bool holdingItem;

    public float maxHoldBuffer; // tiny window, where you can't drop the item
    float holdBuffer;

    // tasks
    public bool taskNear;
    public bool doingTask;


    void Start()
    {
        
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
                velocity = runVelocity * ((windupTime - windup) / windupTime);
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

        item = nearestItem;
        item.GetComponent<Item>().ItemGrabbed();
        holdingItem = true;
        holdBuffer = maxHoldBuffer;
    }

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
