using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    // held or near checking
    public bool closestInteractable = false;
    public bool held = false;
    GameObject player;

    // propreties
    public SpriteRenderer sr;
    public string attribute;
    public Animator animator;

    void Update()
    {
        animator.SetBool("Near", closestInteractable);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.gameObject;
            collision.GetComponent<PlayerControler>().ItemAddProximity(this.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = null;
            collision.GetComponent<PlayerControler>().ItemCloseProximity(this.gameObject);
        }
    }

    public void ItemGrabbed()
    {
        held = true;
        sr.sortingOrder = 0;
        animator.SetBool("Hold", true);
    }

    public void ItemDropped()
    {
        held = false;
        sr.sortingOrder = -3;
        animator.SetBool("Hold", false);
    }
}
