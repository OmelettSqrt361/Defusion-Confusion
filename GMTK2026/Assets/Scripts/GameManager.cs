using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject overlayMenu;
    public int winConditionCount;
    [HideInInspector]
    public int winConditions;

    [HideInInspector]
    public string taskItem = "";

    public Bomb[] bombs;

    [HideInInspector]

    public int minimalBombTime;

    AudioSource audioS;
    public AudioClip beep;
    public AudioClip last10secs;
    int maxBombTime;
    public float maxVolume;
    [HideInInspector]
    public float currentBombFactor;

    bool won;

    private void Start()
    {
        audioS = GetComponent<AudioSource>();

        int iterator = int.MinValue;
        foreach(var bomb in bombs)
        {
            if(bomb.initTimer > iterator)
            {
                iterator = Mathf.CeilToInt(bomb.initTimer);
            }
        }
        maxBombTime = iterator;
    }

    public void Update()
    {
        if(winConditionCount == winConditions && !won)
        {
            Win();
        }

        int searchBomb = int.MaxValue;
        foreach (var bomb in bombs)
        {
            if(searchBomb > bomb.showTimer)
            {
                searchBomb = bomb.showTimer;
            }
        }

        currentBombFactor = (1 - ((float)searchBomb / (float)maxBombTime));

        // check if these two values differ =>
        // either a second passed or a reset was hit either way play sfx
        if (minimalBombTime != searchBomb)
        {
            audioS.volume = currentBombFactor * maxVolume;
            if (minimalBombTime == 10)
            {
                audioS.PlayOneShot(last10secs);
            }
            else if(minimalBombTime > 10)
            {
                audioS.Stop();
                audioS.PlayOneShot(beep);
            }
            minimalBombTime = searchBomb;
        }


    }

    public void HideOverlayMenu()
    {
        overlayMenu.SetActive(false);
    }

    public void ShowOverlayMenu()
    {
        overlayMenu.SetActive(true);
    }

    public void Win()
    {
        Debug.Log("Win!");
        won = true;
    }

    public void Lose()
    {
        Debug.Log("Lose!");
    }

}
