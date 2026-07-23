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

    bool won;

    public void Update()
    {
        if(winConditionCount == winConditions && !won)
        {
            Win();
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
