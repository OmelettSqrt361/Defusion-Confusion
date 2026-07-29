using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class GameManager : MonoBehaviour
{
    public enum sceneTypes { MainMenu, Storyboard, Level };

    public sceneTypes sceneType;

    // gamplay stuff
    public GameObject overlayMenu; // this should be tagged
    public PlayerControler playerControler;

    // wins
    public int winConditionCount;
    public int winConditions;

    public string taskItem = "";

    // audio
    AudioSource audioS;
    AudioClip beep;
    AudioClip last10secs;
    AudioClip boom;
    AudioClip winSFX;

    // bomb management
    public int maxBombTime;
    public float maxVolume;
    public float currentBombFactor;
    public List<Bomb> bombs = new List<Bomb>();
    public int minimalBombTime;

    // ending
    public GameObject deathScreen; // this should be tagged
    public GameObject winScreen; // this should be tagged
    public bool hasEnded;
    public bool notBegun;
    public bool won;

    // time measurement
    public float timer;
    public TextMeshProUGUI timeText; // this should be tagged

    // level specifics
    public MusicManager musicManager;
    public bool hasNewSong;
    public AudioClip newSong;
    public float songVolume = 0.2f;

    public GameObject musicManagerFallback;

    private void Start()
    {
        audioS = gameObject.GetComponent<AudioSource>();

        // only main level scenePropreties
        if(sceneType == sceneTypes.Level)
        {
            playerControler = GameObject.FindWithTag("Player").GetComponent<PlayerControler>();
            overlayMenu = GameObject.FindWithTag("Hideable Overlay");
            deathScreen = FindDisabledWithTag("Death Screen");
            winScreen = FindDisabledWithTag("Win Screen");
            timeText = FindDisabledWithTag("Time Text").GetComponent<TextMeshProUGUI>();
        }
        
        if(GameObject.FindWithTag("Main Audio") == null)
        {
            GameObject newManager = Instantiate(musicManagerFallback, gameObject.transform.position, Quaternion.identity);
            musicManager = newManager.GetComponent<MusicManager>();
        }
        else
        {
            musicManager = GameObject.FindWithTag("Main Audio").GetComponent<MusicManager>();
        }

        beep = musicManager.beep;
        last10secs = musicManager.last10secs;
        boom = musicManager.boom;
        winSFX = musicManager.winSFX;

        notBegun = true; // becuase begenning countdown
        if (sceneType == sceneTypes.Level) { playerControler.notBegun = true; }


        if(sceneType == sceneTypes.Level)
        {
            int iterator = int.MinValue;
            foreach (var bomb in bombs)
            {
                if (bomb.initTimer > iterator)
                {
                    iterator = Mathf.CeilToInt(bomb.initTimer);
                }
            }
            maxBombTime = iterator;
        }

        // audio
        if (hasNewSong)
        {
            musicManager.ChangeSong(newSong, songVolume);
        }
    }

    public void Update()
    {
        if (sceneType == sceneTypes.Level) // only do level stuff
        {
            if (winConditionCount == winConditions && !won)
            {
                Win();
            }

            int searchBomb = int.MaxValue;
            foreach (var bomb in bombs)
            {
                if (searchBomb > bomb.showTimer)
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
                    audioS.Stop();
                    audioS.PlayOneShot(last10secs);
                }
                else if (minimalBombTime > 10)
                {
                    audioS.Stop();
                    audioS.PlayOneShot(beep);
                }
                else if (minimalBombTime < searchBomb)
                {
                    audioS.Stop();
                    audioS.PlayOneShot(last10secs);
                    audioS.time = minimalBombTime;
                }
                minimalBombTime = searchBomb;
            }

            // timer
            if (!notBegun && !hasEnded) { timer += Time.deltaTime; }
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

    public GameObject FindDisabledWithTag(string tag)
    {
        // Finds all objects in memory, including inactive scene objects
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Filter out prefabs or assets saved in the project folder
            if (obj.hideFlags == HideFlags.None && obj.CompareTag(tag))
            {
                return obj;
            }
        }

        return null;
    }

    public void DefuseBomb(Bomb toDestroy)
    {
        // get rid of bomb in the List and don't go through it
        bombs.Remove(toDestroy);

        // reevaluate the timer
        #region Reevaluation of timer
        int iterator = int.MinValue;
        foreach (var bomb in bombs)
        {
            if (bomb.initTimer > iterator)
            {
                iterator = Mathf.CeilToInt(bomb.initTimer);
            }
        }
        maxBombTime = iterator;

        int searchBomb = int.MaxValue;
        foreach (var bomb in bombs)
        {
            if (searchBomb > bomb.showTimer)
            {
                searchBomb = bomb.showTimer;
            }
        }

        currentBombFactor = (1 - ((float)searchBomb / (float)maxBombTime));
        if (minimalBombTime != searchBomb)
        {
            if (!won) { audioS.volume = currentBombFactor * maxVolume; }
            if (minimalBombTime == 10)
            {
                audioS.Stop();
                audioS.PlayOneShot(last10secs);
            }
            else if (minimalBombTime > 10)
            {
                audioS.Stop();
                audioS.PlayOneShot(beep);
            }
            else if (minimalBombTime < searchBomb)
            {
                audioS.Stop();
                audioS.PlayOneShot(last10secs);
                audioS.time = minimalBombTime;
            }
            minimalBombTime = searchBomb;
        }
        #endregion
    }

}
