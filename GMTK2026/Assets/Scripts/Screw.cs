using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Screw : MonoBehaviour
{
    GameManager gm;
    Button activator;
    public string attribute;
    Animator animator;
    ScrewCover cover;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        activator = GetComponent<Button>();
        animator = GetComponent<Animator>();
        cover = GetComponentInParent<ScrewCover>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gm.taskItem == attribute)
        {
            activator.interactable = true;
        }
        else
        {
            activator.interactable = false;
        }
    }

    public void Unscrew()
    {
        cover.screwsUnsrewed++;
        animator.SetTrigger("Unscrew");
    }
}
