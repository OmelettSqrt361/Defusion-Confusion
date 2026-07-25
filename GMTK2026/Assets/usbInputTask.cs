using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class usbInputTask : MonoBehaviour
{

    public Button usbButton;
    public GameObject usbImage;
    public Image diode;
    GameManager gm;
    public string attributeWrong;
    public string attributeRight;
    public Bomb bomb;
    bool done;
    AudioSource audioS;
    public AudioClip good;
    public AudioClip bad;

    void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        audioS = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if((gm.taskItem == attributeRight || gm.taskItem == attributeWrong) && !done)
        {
            usbButton.interactable = true;
        } else
        {
            usbButton.interactable = false;
        }
    }

    public void OnInput()
    {
        if(gm.taskItem == attributeRight)
        {
            done = true;
            gm.winConditions++;
            bomb.bombCoditions++;
            usbButton.interactable = false;
            usbImage.SetActive(true);
            diode.color = Color.green;
            audioS.PlayOneShot(good);
        }
        else
        {
            audioS.PlayOneShot(bad);
        }
    }
}
