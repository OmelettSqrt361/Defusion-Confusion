using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Wire : MonoBehaviour
{

    GameManager gm;
    Button activator;
    public string attribute;
    Animator animator;

    public bool isGoodWire;
    WireHolder holder;

    Image sr;
    public Sprite cutSprite;

    bool cut = false;

    void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        activator = GetComponent<Button>();
        animator = GetComponent<Animator>();
        sr = GetComponent<Image>();

        holder = GetComponentInParent<WireHolder>();
    }

    void Update()
    {
        if (gm.taskItem == attribute && !cut)
        {
            activator.interactable = true;
        }
        else
        {
            activator.interactable = false;
        }
    }

    public void Cut()
    {
        cut = true;
        sr.sprite = cutSprite;
        if (isGoodWire)
        {
            holder.goodWiresCut++;
        }
        else
        {
            holder.BadWireCut();
        }
    }
}
