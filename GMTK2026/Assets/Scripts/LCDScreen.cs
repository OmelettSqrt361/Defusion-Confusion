using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LCDScreen : MonoBehaviour
{
    private void Start()
    {
        Bomb bomb = GetComponentInParent<TaskMenuMain>().controler.gameObject.GetComponent<Bomb>();
        bomb.lCDScreen = gameObject.GetComponent<TextMeshProUGUI>();
    }
}
