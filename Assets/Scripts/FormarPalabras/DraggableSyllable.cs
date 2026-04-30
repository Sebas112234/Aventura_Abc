using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DraggableSyllable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    private WordGameManager manager;
    private Vector3 startPosition;
    private string myValue;
    private CanvasGroup canvasGroup;

    public void Init(string val, WordGameManager mgr) {
        myValue = val;
        manager = mgr;
        GetComponentInChildren<TextMeshProUGUI>().text = val;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
    startPosition = transform.position;
    
    // ESTO ES CLAVE: Al empezar a arrastrar, el objeto deja de bloquear raycasts
    // para que el sistema pueda ver lo que hay DEBAJO de él.
    canvasGroup.blocksRaycasts = false; 
    canvasGroup.alpha = 0.6f;
}

public void OnEndDrag(PointerEventData eventData) {
    // Al soltar, primero revisamos qué hay debajo (gracias a que blocksRaycasts es false)
    GameObject hovered = eventData.pointerEnter;

    if (hovered != null && hovered.CompareTag("SyllableSlot")) {
        manager.OnSyllableDropped(myValue, this);
    } else {
        ReturnToStart();
    }

    // FINALMENTE, reactivamos para que se pueda volver a agarrar
    canvasGroup.blocksRaycasts = true;
    canvasGroup.alpha = 1f;
}

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }


public void ReturnToStart() {
    transform.position = startPosition;
    // Aseguramos que el componente sea interactuable de nuevo
    if(canvasGroup != null) {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}
}