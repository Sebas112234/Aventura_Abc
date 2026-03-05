using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(LineRenderer))]
public class ControlTrazo : MonoBehaviour
{
    [Header("Sampling / detection")]
    public float sampleSpacing = 0.05f;        // distancia entre puntos de la letra
    public float hitRadius = 0.08f;            // radio de tolerancia para colliders
    [Range(0.01f, 1f)] public float requiredPercentage = 0.7f; 

    [Header("Line / visual")]
    public float lineWidth = 0.06f;
    public Color lineColor = Color.blue;
    public Material lineMaterial;

    [Header("Debug")]
    public bool showDebugSamples = false;
    public GameObject debugPointPrefab; // sprite para debug

    [Header("Events")]
    public UnityEvent OnLetterCompleted;

    private List<Vector2> samplePoints = new List<Vector2>();
    private bool[] sampleHit;
    private LineRenderer lr;
    private Camera mainCam;
    private bool isTracing = false;
    private List<Vector3> currentStroke = new List<Vector3>();
    private int hitCount = 0;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.startWidth = lr.endWidth = lineWidth;
        if (lineMaterial != null) lr.material = lineMaterial;
        lr.loop = false;
        lr.useWorldSpace = true;
        lr.startColor = lr.endColor = lineColor;
        mainCam = Camera.main;
    }

    void Start()
    {
        BuildSamplePointsFromChildColliders();
    }

    void BuildSamplePointsFromChildColliders()
    {
        samplePoints.Clear();
        // encontrar todos los colliders en la letra
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);

        foreach (var col in colliders)
        {
            if (col is PolygonCollider2D poly)
            {
                SamplePolygonCollider(poly);
            }
            else
            {
                // fallback
                SampleBounds(col.bounds);
            }
        }

        // elimina puntos duplicados o con colliders adyacentes
        samplePoints = samplePoints.Distinct(new Vector2Comparer(0.0001f)).ToList();
        sampleHit = new bool[samplePoints.Count];
        hitCount = 0;

        if (showDebugSamples && debugPointPrefab != null)
        {
            foreach (var p in samplePoints)
            {
                var go = Instantiate(debugPointPrefab, p, Quaternion.identity, transform);
                go.name = "dbg_sample";
            }
        }
    }

    void SamplePolygonCollider(PolygonCollider2D poly)
    {
        for (int p = 0; p < poly.pathCount; p++)
        {
            Vector2[] path = poly.GetPath(p);
            //transformar puntos locales a world para que el muestreo sea correcto con colliders escaleados
            for (int i = 0; i < path.Length; i++)
            {
                path[i] = poly.transform.TransformPoint(path[i]);
            }
            // limita el muestreo al area del poligono, no solo a su bounding box
            Bounds b = new Bounds(path[0], Vector3.zero);
            foreach (var v in path) b.Encapsulate(v);
            SampleBoundsWithCollider(b, poly);
        }
    }

  

    void SampleBounds(Bounds bounds)
    {
        SampleBoundsWithCollider(bounds, null);
    }
    //por cada punto dentro de los bounds, verifica si colisiona con el collider antes de agregarlo a la lista de puntos a detectar
    void SampleBoundsWithCollider(Bounds bounds, Collider2D optionalCol)
    {
        Vector2 min = bounds.min;
        Vector2 max = bounds.max;
        for (float x = min.x; x <= max.x; x += sampleSpacing)
        {
            for (float y = min.y; y <= max.y; y += sampleSpacing)
            {
                Vector2 p = new Vector2(x, y);
                if (optionalCol != null)
                {
                    if (optionalCol.OverlapPoint(p))
                        samplePoints.Add(p);
                }
                else
                {
                    samplePoints.Add(p);
                }
            }
        }
    }

    void Update()
    {
        // esto permite usar mouse en editor y touch en mobile sin cambiar el código
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) BeginTrace(Input.mousePosition);
        if (Input.GetMouseButton(0)) ContinueTrace(Input.mousePosition);
        if (Input.GetMouseButtonUp(0)) EndTrace();
#else
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) BeginTrace(t.position);
            if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) ContinueTrace(t.position);
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) EndTrace();
        }
#endif
    }

    //se llama al iniciar el trazo, limpia el estado y agrega el primer punto
    void BeginTrace(Vector2 screenPos)
    {
        isTracing = true;
        currentStroke.Clear();
        lr.positionCount = 0;
        AddPointToStroke(screenPos);
    }

    //se llama mientras el trazo continúa, agrega puntos y verifica colisiones
    void ContinueTrace(Vector2 screenPos)
    {
        if (!isTracing) return;
        AddPointToStroke(screenPos);
    }

    //al finalizar el trazo, evalúa si se ha completado la letra
    void EndTrace()
    {
        isTracing = false;
        EvaluateCompletion();
    }

    //añade un punto al trazo actual, actualiza el LineRenderer y verifica colisiones con los puntos de muestra
    void AddPointToStroke(Vector2 screenPos)
    {
        Vector3 world = mainCam.ScreenToWorldPoint(screenPos);


        world.z = transform.position.z;


        if (currentStroke.Count == 0 ||
            Vector3.Distance(currentStroke[currentStroke.Count - 1], world) > 0.01f)
        {
            currentStroke.Add(world);
            lr.positionCount = currentStroke.Count;
            lr.SetPositions(currentStroke.ToArray());
            CheckHitsAtPoint((Vector2)world);
        }

        if (!IsInsideAnyCollider(world))
        {
            ResetTrace();
            return;
        }

    }

    // si el punto actual del trazo no está dentro de ningún collider de la letra, se resetea el trazo para evitar que el usuario trace fuera de la letra
    bool IsInsideAnyCollider(Vector2 worldPoint)
    {
        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        foreach (var col in cols)
        {
            if (col.OverlapPoint(worldPoint))
                return true;
        }
        return false;
    }

    //si el usuario ha salido de la letra, se resetea el trazo y se limpia el estado de los puntos de muestra para que tenga que volver a trazar desde el inicio
    void ResetTrace()
    {
        currentStroke.Clear();
        lr.positionCount = 0;

        for (int i = 0; i < sampleHit.Length; i++)
            sampleHit[i] = false;

        hitCount = 0;
    }



    //revisa si el punto actual del trazo está dentro del radio de algún punto de muestra que aún no ha sido marcado como "hit". Si es así, marca ese punto como "hit" y aumenta el contador de hits.
    void CheckHitsAtPoint(Vector2 worldPoint)
    {
        if (samplePoints == null || samplePoints.Count == 0) return;

        float hr2 = hitRadius * hitRadius;
        for (int i = 0; i < samplePoints.Count; i++)
        {
            if (sampleHit[i]) continue;
            if ((samplePoints[i] - worldPoint).sqrMagnitude <= hr2)
            {
                sampleHit[i] = true;
                hitCount++;
            }
        }
    }

    //al finalizar el trazo, se calcula el porcentaje de puntos de muestra que han sido "hit". Si el porcentaje es mayor o igual al requerido, se dispara el evento de letra completada.
    void EvaluateCompletion()
    {
        if (samplePoints == null || samplePoints.Count == 0) return;
        float percent = (float)hitCount / (float)samplePoints.Count;
        if (percent >= requiredPercentage)
        {
            OnLetterCompleted?.Invoke();
        }
    }

    // elimina puntos de muestra duplicados o muy cercanos entre sí para optimizar la detección. 
    class Vector2Comparer : IEqualityComparer<Vector2>
    {
        float eps;
        public Vector2Comparer(float eps) { this.eps = eps; }
        public bool Equals(Vector2 a, Vector2 b) { return Vector2.SqrMagnitude(a - b) <= eps * eps; }
        public int GetHashCode(Vector2 v) { return 0; }
    }

    // esto dibuja gizmos en la escena para visualizar los puntos de muestra y cuáles han sido "hit". Los puntos "hit" se dibujan en verde, mientras que los no "hit" se dibujan en amarillo. Esto es útil para depurar y ajustar el sistema de detección.
    void OnDrawGizmosSelected()
    {
        if (!showDebugSamples || samplePoints == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < samplePoints.Count; i++)
        {
            Gizmos.color = (sampleHit != null && i < sampleHit.Length && sampleHit[i]) ? Color.green : Color.yellow;
            Gizmos.DrawSphere(samplePoints[i], sampleSpacing * 0.2f);
        }
    }
}
