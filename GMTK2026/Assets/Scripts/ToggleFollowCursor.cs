using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ToggleFollowCursor : MonoBehaviour, IPointerClickHandler
{
    private bool isFollowing = false;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    public string attribute;
    GameManager gm;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
    }

    void Update()
    {
        if (isFollowing)
        {
            // Convert screen point directly to World Position
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out Vector3 worldPoint))
            {
                transform.position = worldPoint;
            }

            // Check for click while holding
            if (Input.GetMouseButtonDown(0))
            {
                // Drop only if we didn't click on a UI Button underneath
                if (!IsHoveringButton())
                {
                    StopFollowing();
                }
            }

            gm.taskItem = attribute;
        }
        else
        {
            gm.taskItem = "";
        }
    }

    // Called when clicking the item initially to pick it up
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isFollowing)
        {
            StartFollowing();
        }
    }

    private void StartFollowing()
    {
        isFollowing = true;
        // Let raycasts pass through this item to UI elements underneath
        canvasGroup.blocksRaycasts = false;
    }

    private void StopFollowing()
    {
        isFollowing = false;
        // Re-enable raycasts so the item can be clicked/picked up again
        canvasGroup.blocksRaycasts = true;
    }

    private bool IsHoveringButton()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // Check if any UI element under the cursor is a Button
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponentInParent<Button>() != null)
            {
                return true;
            }
        }

        return false;
    }
}