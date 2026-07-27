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
    public GameObject usbTaskItem;

    Color enableColor;
    Color transparent = new Color(1f, 1f, 1f, 0f);

    void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        audioS = gameObject.GetComponent<AudioSource>();
        enableColor = usbButton.colors.normalColor;

        ColorBlock cb = usbButton.colors;
        cb.normalColor = transparent;
        cb.pressedColor = transparent;
        usbButton.colors = cb;
    }

    // Update is called once per frame
    void Update()
    {
        if((gm.taskItem == attributeRight || gm.taskItem == attributeWrong) && !done)
        {
            ColorBlock cb = usbButton.colors;
            cb.normalColor = enableColor;
            cb.pressedColor = enableColor;
            usbButton.colors = cb;
            usbButton.interactable = true;

        } else
        {
            ColorBlock cb = usbButton.colors;
            cb.normalColor = transparent;
            cb.pressedColor = transparent;
            usbButton.colors = cb;
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

            GameObject item = gm.playerControler.item;
            gm.playerControler.ItemDrop();
            item.SetActive(false);
            usbTaskItem.SetActive(false);
        }
        else
        {
            audioS.PlayOneShot(bad);
        }
    }
}
