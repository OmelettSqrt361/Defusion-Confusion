using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lock : MonoBehaviour
{
    GameManager gm;
    Button activator;
    public string attribute;
    Animator animator;
    Door door;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        activator = GetComponent<Button>();
        animator = GetComponentInParent<Animator>();
        door = GetComponentInParent<TaskMenuMain>().controler.gameObject.GetComponent<Door>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gm.taskItem == attribute)
        {
            activator.interactable = true;
        }
        else
        {
            activator.interactable = false;
        }
    }

    public void UnlockAnim()
    {
        animator.SetTrigger("Unlock");
    }

    public void Unlocked()
    {
        door.Unlock();
    }
}
