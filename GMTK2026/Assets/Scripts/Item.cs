using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    bool playerNear = false;
    public bool held = false;
    GameObject player;

    public Animator animator;

    void Update()
    {
        animator.SetBool("Near", playerNear);
        if (playerNear && !held)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                animator.SetTrigger("Hold");
                player.GetComponent<PlayerControler>().ItemPickup(this.gameObject);
                held = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Can pickup item");
            playerNear = true;
            player = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNear = false;
            player = null;
        }
    }

    public void ItemDropped()
    {
        held = false;
        animator.SetTrigger("Hold");
    }
}
