using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject overlayMenu;

    public void HideOverlayMenu()
    {
        overlayMenu.SetActive(false);
    }

    public void ShowOverlayMenu()
    {
        overlayMenu.SetActive(true);
    }

}
