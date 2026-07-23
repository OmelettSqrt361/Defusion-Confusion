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

    private void Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void OpenBox()
    {
        animator.SetTrigger("Open");
    }

    public void SpawnItem()
    {
        Instantiate(spawnedItem, new Vector3(spawnpoint.position.x, spawnpoint.position.y, 0), Quaternion.identity);
        sr.sprite = emptyBox;
    }
}
