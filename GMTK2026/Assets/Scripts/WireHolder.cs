using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireHolder : MonoBehaviour
{

    public int goodWiresCount;
    [HideInInspector]
    public int goodWiresCut;
    GameManager gm;

    public Bomb bomb;

    [HideInInspector]
    public bool done;

    void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(goodWiresCount == goodWiresCut && !done)
        {
            gm.winConditions++;
            bomb.bombCoditions++;
            done = true;
        }
    }

    public void BadWireCut()
    {
        gm.Lose();
    }
}
