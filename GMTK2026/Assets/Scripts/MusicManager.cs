using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    AudioSource audioS;

    // A list of sounds
    [Header("List of SFX")]
    public AudioClip beep;
    public AudioClip last10secs;
    public AudioClip boom;
    public AudioClip winSFX;

    void Awake()
    {
        if(GameObject.FindWithTag("Main Audio").GetComponent<GameManager>() == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        audioS = GetComponent<AudioSource>();
    }

    public void ChangeSong(AudioClip newSong, float volume)
    {
        if (audioS.clip != newSong)
        {
            audioS.clip = newSong;
            audioS.volume = volume;
            audioS.Play();
        }
    }
}
