using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxOpener : MonoBehaviour
{

    Animator animator;
    SpriteRenderer sr;
    public Sprite emptyBox;
    public GameObject spawnedItem;
    public Transform spawnpoint;
    GameManager gm;

    private void Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
    }

    public void OpenBox()
    {
        animator.SetTrigger("Open");
    }

    public void SpawnItem()
    {
        GameObject spawned = Instantiate(spawnedItem, new Vector3(spawnpoint.position.x, spawnpoint.position.y, 0), Quaternion.identity);
        gm.AddOutlinedObject("item", spawned);

        sr.sprite = emptyBox;
    }
}
