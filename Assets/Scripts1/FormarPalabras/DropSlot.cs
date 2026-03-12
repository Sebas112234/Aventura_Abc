using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;  

public class DropSlot : MonoBehaviour, IDropHandler
{
    // Este evento avisará al GameManager cuando algo se suelte aquí
    public delegate void OnItemDropped(GameObject droppedItem);
    public event OnItemDropped OnItemDroppedEvent;

    public void OnDrop(PointerEventData eventData)
    {
        // Verificar si estamos arrastrando algo
        if (eventData.pointerDrag != null)
        {
            // Avisar que se soltó un objeto aquí
            OnItemDroppedEvent?.Invoke(eventData.pointerDrag);
        }
    }
}