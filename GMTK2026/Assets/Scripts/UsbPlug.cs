using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UsbPlug : MonoBehaviour
{
    GameManager gm;
    Button activator;
    public GameObject usb;
    public GameObject usbButton;
    public string attribute;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        activator = GetComponent<Button>();
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

    public void KillUsb()
    {
        GameObject item = gm.playerControler.item;

        gm.playerControler.ItemDrop();
        item.SetActive(false);
        usb.SetActive(false);
        usbButton.SetActive(false);
    }

}
