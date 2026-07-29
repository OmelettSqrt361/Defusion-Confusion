using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ToggleFollowCursor : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private bool isFollowing = false;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    public string attribute;
    private GameManager gm;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        GameObject gc = GameObject.FindWithTag("GameController");
        if (gc != null)
        {
            gm = gc.GetComponent<GameManager>();
        }
    }

    void Update()
    {
        if (isFollowing)
        {
            // Follow cursor position
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rectTransform,
                Input.mousePosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector3 worldPoint))
            {
                transform.position = worldPoint;
            }

            // Drop item on next left click (ignoring the frame it was picked up)
            if (Input.GetMouseButtonDown(0))
            {
                if (!IsHoveringButton())
                {
                    StopFollowing();
                }
            }

            if (gm != null) gm.taskItem = attribute;
        }
        else
        {
            if (gm != null && gm.taskItem == attribute) gm.taskItem = "";
        }
    }

    // Handles normal click to pick up
    public void OnPointerClick(PointerEventData eventData)
    {
        // Avoid toggling off on the same click that picks it up via Drag
        if (eventData.dragging) return;

        if (!isFollowing)
        {
            StartFollowing();
        }
    }

    // Handles click-and-drag mechanics
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isFollowing)
        {
            StartFollowing();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Interface required by Unity to process drag events properly
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Releasing the mouse button after dragging keeps 'isFollowing' true,
        // so it continues to follow the cursor until the next click.
    }

    private void StartFollowing()
    {
        isFollowing = true;
        // Allows mouse clicks to pass through the object to UI underneath
        canvasGroup.blocksRaycasts = false;
    }

    private void StopFollowing()
    {
        isFollowing = false;
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

        foreach (RaycastResult result in results)
        {
            if (result.gameObject != gameObject && result.gameObject.GetComponentInParent<Button>() != null)
            {
                return true;
            }
        }

        return false;
    }
}