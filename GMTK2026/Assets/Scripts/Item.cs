using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    // held or near checking
    bool playerNear = false;
    public bool held = false;
    GameObject player;

    // propreties
    public SpriteRenderer sr;
    public string attribute;
    public Animator animator;

    void Update()
    {
        animator.SetBool("Near", playerNear);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNear = true;
            player = collision.gameObject;
            collision.GetComponent<PlayerControler>().ItemAddProximity(this.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNear = false;
            player = null;
            collision.GetComponent<PlayerControler>().ItemCloseProximity(this.gameObject);
        }
    }

    public void ItemGrabbed()
    {
        held = true;
        sr.sortingOrder = 0;
        animator.SetTrigger("Hold");
    }

    public void ItemDropped()
    {
        held = false;
        sr.sortingOrder = -3;
        animator.SetTrigger("Hold");
    }
}
