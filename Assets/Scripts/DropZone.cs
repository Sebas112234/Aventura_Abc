using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public MathGameUI gameUI;

    public void OnDrop(PointerEventData eventData)
    {
        DraggableOption option =
            eventData.pointerDrag.GetComponent<DraggableOption>();

        if (option != null)
        {
            option.MarkAsDropped();
            gameUI.CheckAnswer(option.value);
            option.ResetPosition();
        }
    }
}