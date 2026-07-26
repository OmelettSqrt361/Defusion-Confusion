using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class GameManager : MonoBehaviour
{

    // gamplay stuff
    public GameObject overlayMenu;
    [HideInInspector]
    public PlayerControler playerControler;

    // wins
    public int winConditionCount;
    [HideInInspector]
    public int winConditions;

    [HideInInspector]
    public string taskItem = "";

    // audio
    public AudioSource audioS;
    public AudioClip beep;
    public AudioClip last10secs;
    public AudioClip boom;
    public AudioClip winSFX;

    // bomb management
    int maxBombTime;
    public float maxVolume;
    [HideInInspector]
    public float currentBombFactor;
    public Bomb[] bombs;
    [HideInInspector]
    public int minimalBombTime;

    // ending
    public GameObject deathScreen;
    public GameObject winScreen;
    [HideInInspector]
    public bool hasEnded;
    [HideInInspector]
    public bool notBegun;
    [HideInInspector]
    public bool won;

    // time measurement
    [HideInInspector]
    public float timer;
    public TextMeshProUGUI timeText;

    private void Start()
    {
        audioS = gameObject.GetComponent<AudioSource>();
        playerControler = GameObject.FindWithTag("Player").GetComponent<PlayerControler>();

        notBegun = true; // becuase begenning countdown
        playerControler.notBegun = true;

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
            if (!won) { audioS.volume = currentBombFactor * maxVolume; }
            if (minimalBombTime == 10)
            {
                audioS.PlayOneShot(last10secs);
            }
            else if(minimalBombTime > 10)
            {
                audioS.Stop();
                audioS.PlayOneShot(beep);
            } else if (minimalBombTime < searchBomb)
            {
                audioS.PlayOneShot(last10secs);
                audioS.time = minimalBombTime;
            }
            minimalBombTime = searchBomb;
        }

        // timer
        if (!notBegun && !hasEnded) { timer += Time.deltaTime; }
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
        hasEnded = true;
        playerControler.hasEnded = true;

        // stop audio
        audioS.Stop();
        StopAllAudio();
        audioS.volume = maxVolume;
        audioS.PlayOneShot(winSFX);


        // open win screen
        winScreen.SetActive(true);
        timeText.text = $"Time: {Mathf.FloorToInt(timer / 60)}:" + (Mathf.FloorToInt(timer) % 60).ToString("D2") + $".{Mathf.FloorToInt(timer * 1000) % 1000}";
        Debug.Log($"{timer}");
        won = true;
    }

    public void Lose()
    {
        hasEnded = true;
        playerControler.hasEnded = true;

        // delete all bombs
        foreach (var bomb in bombs)
        { 
            bomb.gameObject.SetActive(false);
        }

        // stop audio
        GameObject.FindWithTag("Main Audio").GetComponent<AudioSource>().Stop();
        audioS.Stop();
        StopAllAudio();
        
        audioS.volume = maxVolume;
        audioS.PlayOneShot(boom);

        //open deathscreen
        deathScreen.SetActive(true);
    }

    public void StopAllAudio()
    {
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();

        foreach (AudioSource audioSource in allAudioSources)
        {

            if(audioSource != audioS && audioSource.gameObject.tag != "Main Audio")
            {
                audioSource.Stop();
            }
        }
    }

    public void ForceBeepStop()
    {
        audioS.Stop();
    }

}
