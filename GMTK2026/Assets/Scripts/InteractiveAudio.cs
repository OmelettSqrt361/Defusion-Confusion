using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveAudio : MonoBehaviour
{

    AudioSource audioS;
    Transform player;
    public float hearingZenith;
    public bool pan;
    public float maxSound;


    // Start is called before the first frame update
    void Start()
    {
        audioS = GetComponent<AudioSource>();
        player = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        float playerDist = Mathf.Sqrt(Mathf.Pow(transform.position.x - player.position.x,2) + Mathf.Pow(transform.position.y - player.position.y, 2));
        audioS.volume = (1 - (playerDist / hearingZenith)) * maxSound;
        if (pan) { audioS.panStereo = (transform.position.x - player.position.x)/hearingZenith; }
    }
}
