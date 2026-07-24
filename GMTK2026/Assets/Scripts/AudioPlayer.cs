using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{

    AudioSource audioS;
    public AudioClip clip;

    void Start()
    {
        audioS = GetComponent<AudioSource>();
    }

    public void PlayAudioCustom(AudioClip newClip)
    {
        audioS.PlayOneShot(newClip);
    }

    public void PlayAudio()
    {
        audioS = GetComponent<AudioSource>();
        audioS.PlayOneShot(clip);
    }
}
