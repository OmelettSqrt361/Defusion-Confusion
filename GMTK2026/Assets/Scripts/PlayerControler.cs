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
    public Transform handLoc;
    public GameObject item;
    bool holdingItem;

    public float maxHoldBuffer; // tiny window, where you can't drop the item
    float holdBuffer;


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

        animator.SetFloat("Velocity", velocity); // animation player

        if (horizontal == 0 && vertical == 0)
        {
            windup = windupTime;
            velocity = 0;
        }
        else if(windup > 0)
        {
            windup = windup - Time.deltaTime;
            velocity = runVelocity * ((windupTime - windup) / windupTime);
        }

        moveDir = moveDir * velocity;

        // item management
        
        if(holdBuffer > 0)
        {
            holdBuffer -= Time.deltaTime;
        }

        if(holdingItem)
        {
            item.transform.position = handLoc.position;
            if (Input.GetKeyDown(KeyCode.X) && holdBuffer <= 0)
            {
                ItemDrop();
            }
        }
    }

    void FixedUpdate()
    {
        rb.velocity = moveDir;
        // movement logic
    }

    public void ItemDrop()
    {
        Debug.Log("Dropped the item");
        item.GetComponent<Item>().ItemDropped();
        item = null;
        holdingItem = false;
    }

    public void ItemPickup(GameObject newItem)
    {
        Debug.Log("Picked Up Item");
        holdingItem = true;
        item = newItem;
        holdBuffer = maxHoldBuffer;
    }
}
