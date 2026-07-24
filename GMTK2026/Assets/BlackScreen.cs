using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackScreen : MonoBehaviour
{

    float timer = 0;
    bool isOn;
    Image img;

    void Start()
    {
        img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        } else if(isOn)
        {
            TurnOff();
        }
    }

    public void TurnOn(float setTimer)
    {
        isOn = true;
        timer = setTimer;
        img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
    }

    void TurnOff()
    {
        isOn = false;
        timer = 0;
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
    }
}
