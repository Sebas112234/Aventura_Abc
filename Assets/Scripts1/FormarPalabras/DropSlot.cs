using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public delegate void OnItemDropped(GameObject droppedItem);
    public event OnItemDropped OnItemDroppedEvent;

    public void OnDrop(PointerEventData eventData) {
        if (eventData.pointerDrag != null) {
            OnItemDroppedEvent?.Invoke(eventData.pointerDrag);
        }
    }
}