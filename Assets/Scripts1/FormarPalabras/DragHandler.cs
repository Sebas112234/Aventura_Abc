using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Guardar estado original
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        // Mover temporalmente al Canvas raíz para que no lo corten los paneles
        transform.SetParent(transform.root);
        
        // Hacerlo semi-transparente e ignorar bloqueos de raycast
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Seguir el ratón/dedo
        rectTransform.anchoredPosition += eventData.delta / transform.root.GetComponent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restaurar estado
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Si no se soltó en un drop zone válido, volver a su sitio
        if (transform.parent == transform.root)
        {
            RegresarAPosicionOriginal();
        }
    }

    public void RegresarAPosicionOriginal()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
}