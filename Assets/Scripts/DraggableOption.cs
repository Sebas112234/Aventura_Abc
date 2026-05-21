using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableOption : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 startPosition;
    private bool droppedCorrectly = false;

    public int value;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = rectTransform.anchoredPosition;
        droppedCorrectly = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!droppedCorrectly)
        {
            ResetPosition();
        }
    }

    public void MarkAsDropped()
    {
        droppedCorrectly = true;
    }

    public void ResetPosition()
    {
        rectTransform.anchoredPosition = startPosition;
    }
}