using UnityEngine;

public class ValidarTrazo : MonoBehaviour
{
    public float RadioTolerancia = 0.6f;   // tolerancia para niños
    public LayerMask TrazoLayer;          // capa de los colliders
    public float DistanciaTrazo = 1.5f;      // distancia máxima antes de fallar

    private Camera mainCam;
    private bool tracing = false;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 worldPos = mainCam.ScreenToWorldPoint(touch.position);

            if (touch.phase == TouchPhase.Began)
            {
                tracing = true;
            }

            if (touch.phase == TouchPhase.Moved && tracing)
            {
                ValidarTouch(worldPos);
            }

            if (touch.phase == TouchPhase.Ended)
            {
                tracing = false;
            }
        }

        
    }

    void ValidarTouch(Vector2 position)
    {
        // Círculo invisible de tolerancia
        Collider2D hit = Physics2D.OverlapCircle(position, RadioTolerancia, TrazoLayer);

        if (hit != null)
        {
            Debug.Log("Correcto");
        }
        else
        {
            Debug.Log("Fuera del trazo");

            // Opcional: aquí puedes reiniciar el trazo
        }
    }

    // Para ver el radio en el editor
    void OnDrawGizmos()
    {
        if (Application.isPlaying && mainCam != null)
        {
            Vector2 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(mouseWorld, RadioTolerancia);
        }
    }
}
