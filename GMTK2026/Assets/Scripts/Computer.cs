using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml.Serialization;
using JetBrains.Annotations;

public class Computer : MonoBehaviour
{

    public GameObject computerTask;
    Animator computerTaskAnim;

    int state;
    // current state:
    // 0 - no password
    // 1 - message
    // 2 - dowloading
    // 3 - connection lost
    // 4 - take usb
    // 5 - blank


    public TMP_InputField inputField;
    public Slider downloadSlider;
    public bool usbPlugged;

    public float maxTimer;
    public float stopTime;
    float timer;
    bool isTimer;
    bool hickup;

    public Image computerSR;
    public Sprite computerUnplugged;
    public Sprite computerPlugged;

    public GameObject usbFull;
    public Transform spawnpoint;
    AudioSource audioS;
    public AudioClip error;
    public AudioClip login;

    private void Start()
    {
        audioS = GetComponent<AudioSource>();
        computerTask.GetComponent<ComputerTask>().headComputer = this;
        computerTaskAnim = computerTask.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        SetState(state);
        if(inputField.text.Length == 5)
        {
            if(inputField.text == "21885")
            {
                computerTaskAnim.SetTrigger("Next");
                inputField.text = "";
                audioS.PlayOneShot(login);
                SetState(1);
                computerTaskAnim.SetBool("Open Computer", true);
            }
            else
            {
                computerTaskAnim.SetTrigger("Wrong");
                inputField.text = "";
            }
        }

        if (isTimer)
        {
            timer += Time.deltaTime;
            downloadSlider.value = timer / maxTimer;
            if (timer >= stopTime && !hickup)
            {
                isTimer = false;
                hickup = true;
                audioS.PlayOneShot(error);
                SetState(3);
            } else if(timer >= maxTimer)
            {
                isTimer = false;
                SetState(4);
            }
        }
    }

    public void NextConnection()
    {
        computerTaskAnim.SetTrigger("Next");
        isTimer = true;
        SetState(2);
    }

    public void Next()
    {
        computerTaskAnim.SetTrigger("Next");
    }

    public void UsbFull()
    {
        Instantiate(usbFull, spawnpoint.position, Quaternion.identity);
    }

    public void plugInUsb()
    {
        computerSR.sprite = computerPlugged;
        usbPlugged = true;
    }

    public void NextIfUsb()
    {
        if (usbPlugged)
        {
            computerTaskAnim.SetTrigger("Next");
            SetState(2);
            isTimer = true;
        }
        else
        {
            computerTaskAnim.SetTrigger("Wrong");
        }
    }

    public void ActivateTimer()
    {
        isTimer = true;
    }

    public void SetState(int newState)
    {
        state = newState;
        computerSR.sprite = computerUnplugged;
        if (computerTaskAnim.gameObject.activeSelf) { computerTaskAnim.SetInteger("State", newState); }
    }

    public void CancelOpenComputer()
    {
        computerTaskAnim.SetBool("Open Computer", false);
    }
}
