using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    AudioSource audioS;

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
    }

    private void Start()
    {
        audioS = GetComponent<AudioSource>();
    }

    public void ChangeSong(AudioClip newSong)
    {
        audioS.clip = newSong;
    }
}
