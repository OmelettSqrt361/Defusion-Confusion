using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountDown : MonoBehaviour
{

    GameManager gm;
    Animator animator;

    // Start is called before the first frame update
    void Awake()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        animator = gameObject.GetComponent<Animator>();
    }

    public void OnCountdownEnd()
    {
        gm.notBegun = false;
        gm.playerControler.notBegun = false;
        animator.SetBool("Complete", true);
    }
}
