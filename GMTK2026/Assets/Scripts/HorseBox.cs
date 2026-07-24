using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorseBox : MonoBehaviour
{

    public Image[] imgs;

    public Sprite[] nonHorses;
    public Sprite horse;
    public Sprite[] last3;

    public Image header;
    public Sprite normalHeader;
    public Sprite squidHeader;

    public int maxTurns;
    int turns = 0;
    int chosen = 0;

    private int lastHorseIndex = -1;

    Task bombTask;
    public Bomb bomb;
    Image bg;
    public Sprite winSprite;

    void Start()
    {
        bg = GetComponent<Image>();
        bombTask = bomb.gameObject.GetComponent<Task>();
    }

    public void StartRound()
    {
        turns++;
        header.sprite = normalHeader;
        foreach (var item in imgs)
        {
            item.color = Color.white;
        }

        do
        {
            chosen = Random.Range(0, imgs.Length);
        }
        while (chosen == lastHorseIndex && imgs.Length > 1);

        lastHorseIndex = chosen;
        imgs[chosen].sprite = horse;
        List<int> availableNonHorseIndices = new List<int>();
        for (int i = 0; i < nonHorses.Length; i++)
        {
            availableNonHorseIndices.Add(i);
        }

        for (int i = 0; i < imgs.Length; i++)
        {
            if (i == chosen) continue;

            int randomIndex = Random.Range(0, availableNonHorseIndices.Count);
            int chosenSpriteIndex = availableNonHorseIndices[randomIndex];

            imgs[i].sprite = nonHorses[chosenSpriteIndex];
            availableNonHorseIndices.RemoveAt(randomIndex);
        }
    }

    void SillyRound()
    {
        turns++;
        header.sprite = squidHeader;
        imgs[0].sprite = last3[0];
        imgs[1].sprite = last3[1];
        imgs[2].sprite = last3[2];
        chosen = 2;
    }

    public void Select(int selection)
    {
        if(selection != chosen)
        {
            Debug.Log("Bad Luck!");
            turns = 0;
        }

        if (turns > maxTurns)
        {
            bombTask.ZoomOut();
            bg.sprite = winSprite;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            bomb.bombCoditions++;
            Debug.Log("Wins");

        }
        else if (turns == maxTurns) 
        { 
            SillyRound();
        } 
        else
        {
            StartRound();
        }
    }
}
