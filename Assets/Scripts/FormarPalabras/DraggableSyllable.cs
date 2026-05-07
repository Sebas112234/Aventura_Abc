using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DraggableSyllable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    private Vector3 startPosition;
    private Transform startParent;
    private CanvasGroup canvasGroup;
    private string syllableValue;
    private WordGameManager manager;
    private Canvas mainCanvas;

    public void Init(string val, WordGameManager mgr) {
        syllableValue = val;
        manager = mgr;
        GetComponentInChildren<TextMeshProUGUI>().text = val;
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GetComponentInParent<Canvas>();
        startPosition = transform.position;
        startParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        startPosition = transform.position; // Actualizamos por si el layout se movió
        transform.SetParent(mainCanvas.transform); 
    }

    public void OnDrag(PointerEventData eventData) {
        // SOLUCIÓN DEFINITIVA AL DESFASE:
        // Convertimos la posición del mouse directamente a un punto en el mundo de la UI
        Vector3 worldPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            mainCanvas.transform as RectTransform, 
            eventData.position, 
            mainCanvas.worldCamera, 
            out worldPoint
        );
        transform.position = worldPoint;
    }

    public void OnEndDrag(PointerEventData eventData) {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // NUEVA DETECCIÓN SIN TAGS:
        // Buscamos si debajo del mouse hay un objeto que tenga el script SyllableSlot
        GameObject hitObject = eventData.pointerEnter;
        
        if (hitObject != null) {
            SyllableSlot slot = hitObject.GetComponentInParent<SyllableSlot>();
            
            // Si el objeto tiene el script y además ES el hueco faltante
            if (slot != null && slot.isPlaceholder) {
                manager.OnSyllableDropped(syllableValue, this);
                return;
            }
        }

        // Si no cayó en el lugar correcto, vuelve a su sitio
        ReturnToStart();
    }

    public void ReturnToStart() {
        transform.SetParent(startParent);
        transform.position = startPosition;
    }
}