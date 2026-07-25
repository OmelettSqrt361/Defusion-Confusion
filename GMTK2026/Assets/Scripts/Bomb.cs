using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Bomb : MonoBehaviour
{

    // time tracking
    public float initTimer;
    float timer;
    [HideInInspector]
    public int showTimer;
    float percent;


    // bomb menu
    public Slider slider;
    TextMeshProUGUI countDown;
    public TextMeshProUGUI lCDScreen;

    // win conditions
    public int bombConditionCount;
    [HideInInspector]
    public int bombCoditions;
    GameManager gm;
    Task thisBombTask;

    //losing
    bool detonated = false;


    void Start()
    {
        timer = initTimer;
        countDown = slider.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        thisBombTask = gameObject.GetComponent<Task>();
    }

    // Update is called once per frame
    void Update()
    {
        if (bombCoditions != bombConditionCount && gm.hasEnded == false)
        {

            if (!gm.notBegun)
            {
                if (timer >= 0) { timer -= Time.deltaTime; }
            }

            showTimer = Mathf.RoundToInt(timer);
            percent = showTimer / initTimer;
            string lcdScreen = $"{showTimer / 60}:" + (showTimer % 60).ToString("D2");

            slider.value = percent;
            countDown.text = showTimer.ToString();
            lCDScreen.text = lcdScreen;

            if (timer <= 0 && !detonated)
            { 
                Detonate();
            }
        }
        else if(bombCoditions == bombConditionCount)
        {
            if(slider != null) { Destroy(slider.gameObject); }
            lCDScreen.text = "SAFE";
            if (gm.won)
            {
                thisBombTask.TurnOff();
            }
        }
    }

    public void Detonate()
    {
        detonated = true;
        gm.Lose();
        thisBombTask.TurnOff();
    }

    public void ResetTimer()
    {
        timer = initTimer;
    }
}
