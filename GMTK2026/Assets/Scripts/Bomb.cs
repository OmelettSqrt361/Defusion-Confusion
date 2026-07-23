using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Bomb : MonoBehaviour
{
    public float initTimer;
    float timer;
    int showTimer;
    float percent;

    public Slider slider;
    public TextMeshProUGUI countDown;

    public TextMeshProUGUI lCDScreen;


    void Start()
    {
        timer = initTimer;
    }

    // Update is called once per frame
    void Update()
    {
        if(timer >= 0) { timer -= Time.deltaTime;  }
        showTimer = Mathf.RoundToInt(timer);
        percent = showTimer / initTimer;


        string lcdScreen = $"{showTimer/60}:" + (showTimer%60).ToString("D2");

        slider.value = percent;
        countDown.text = showTimer.ToString();
        lCDScreen.text = lcdScreen; 

        if(timer <= 0)
        {
            Detonate();
        }
    }

    public void Detonate()
    {
        Debug.Log("Detonation!");
    }

    public void ResetTimer()
    {
        timer = initTimer;
    }
}
