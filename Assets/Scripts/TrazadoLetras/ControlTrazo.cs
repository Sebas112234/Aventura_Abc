using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(LineRenderer))]
public class ControlTrazo : MonoBehaviour
{
    [Header("Sampling / detection")]
    public float sampleSpacing = 0.05f;
    public float hitRadius = 0.08f;
    [Range(0.01f, 1f)] public float requiredPercentage = 0.7f;

    [Header("Line / visual")]
    public float lineWidth = 0.06f;
    public Color lineColor = Color.blue;
    public Material lineMaterial;

    [Header("Debug")]
    public bool showDebugSamples = false;
    public GameObject debugPointPrefab;

    [Header("Events")]
    public UnityEvent OnLetterCompleted;

    [Header("Manager")]
    public ManagerLetra managerLetra;
    public int indiceLetra;

    private List<Vector2> samplePoints = new List<Vector2>();
    private bool[] sampleHit;
    private LineRenderer lr;
    private LineRenderer currentLine;
    private Transform linePrefab;
    private Camera mainCam;
    private bool isTracing = false;
    private List<Vector3> currentStroke = new List<Vector3>();
    private int hitCount = 0;
    private bool yaCompletada = false;

    public GameObject MensajeReintentar; 
    public GameObject MensajeFallo;
    private bool mostrandoReintento = false;
    private int fallos = 0;

    private bool limiteActivado = false;


    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;

        lr.positionCount = 0;
        lr.startWidth = lr.endWidth = lineWidth;
        if (lineMaterial != null) lr.material = lineMaterial;
        lr.loop = false;
        lr.useWorldSpace = true;
        lr.startColor = lr.endColor = lineColor;

        linePrefab = transform.Find("LinePrefab");
        mainCam = Camera.main;
    }

    void Start()
    {
        BuildSamplePointsFromChildColliders();
    }

    void BuildSamplePointsFromChildColliders()
    {
        samplePoints.Clear();
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);

        foreach (var col in colliders)
        {
            if (col is PolygonCollider2D poly)
            {
                SamplePolygonCollider(poly);
            }
            else
            {
                SampleBounds(col.bounds);
            }
        }

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
            for (int i = 0; i < path.Length; i++)
            {
                path[i] = poly.transform.TransformPoint(path[i]);
            }
            Bounds b = new Bounds(path[0], Vector3.zero);
            foreach (var v in path) b.Encapsulate(v);
            SampleBoundsWithCollider(b, poly);
        }
    }

    void SampleBounds(Bounds bounds)
    {
        SampleBoundsWithCollider(bounds, null);
    }

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
        if (mostrandoReintento) return;

        if (fallos >= 10 && !limiteActivado)
        {
            limiteActivado = true;
            StartCoroutine(LimiteIntentos());
            return;
        }

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

    void BeginTrace(Vector2 screenPos)
    {
        //Debug.Log(linePrefab == null ? "SIN PREFAB" : "CON PREFAB");
        isTracing = true;
        currentStroke = new List<Vector3>();

        GameObject nuevaLinea;

        if (linePrefab != null)
        {
            nuevaLinea = Instantiate(linePrefab.gameObject, transform);
            currentLine = nuevaLinea.GetComponent<LineRenderer>();

            if (currentLine == null)
                currentLine = nuevaLinea.AddComponent<LineRenderer>();

            currentLine.positionCount = 0;
            //currentLine.startWidth = currentLine.endWidth = lineWidth;
            if (lineMaterial != null) currentLine.material = lineMaterial;
            currentLine.loop = false;
            currentLine.useWorldSpace = true;
            currentLine.startColor = currentLine.endColor = lineColor;
        }
        else
        {
            nuevaLinea = new GameObject("LineaTrazo");
            nuevaLinea.transform.SetParent(transform);
            currentLine = nuevaLinea.AddComponent<LineRenderer>();

            currentLine.sortingLayerName = "Default";
            currentLine.sortingOrder = 10;
        }

        AddPointToStroke(screenPos);
    }

    void ContinueTrace(Vector2 screenPos)
    {
        if (!isTracing) return;
        AddPointToStroke(screenPos);
    }

    void EndTrace()
    {
        isTracing = false;
        EvaluateCompletion();
    }

    void AddPointToStroke(Vector2 screenPos)
    {
        Vector3 world = mainCam.ScreenToWorldPoint(screenPos);
        world.z = transform.position.z;

        if (currentStroke.Count == 0 ||
            Vector3.Distance(currentStroke[currentStroke.Count - 1], world) > 0.01f)
        {
            currentStroke.Add(world);

            if (currentLine != null)
            {
                currentLine.positionCount = currentStroke.Count;
                currentLine.SetPositions(currentStroke.ToArray());
            }

            CheckHitsAtPoint((Vector2)world);
        }

        if (currentStroke.Count > 1 && !IsInsideAnyCollider(world))
        {
            if (!mostrandoReintento)
                StartCoroutine(MostrarReintento());

            return;
        }
    }

    IEnumerator MostrarReintento()
    {
        mostrandoReintento = true;
        isTracing = false;

        ManagerLetra manager = FindObjectOfType<ManagerLetra>();

        MensajeReintentar.transform.SetAsLastSibling();
        currentLine.sortingOrder = -10;
        MensajeReintentar.gameObject.SetActive(true);
        fallos++;

        yield return new WaitForSeconds(1.8f);

        MensajeReintentar.gameObject.SetActive(false);

        if (manager != null)
            manager.ReintentarLetra();
        else
            ResetTrace();

        mostrandoReintento = false;
    }

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

    void ResetTrace()
    {
        currentStroke.Clear();

        if (currentLine != null && currentLine != lr)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
        }
    }

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

    void EvaluateCompletion()
    {
        if (samplePoints == null || samplePoints.Count == 0 || yaCompletada) return;

        float percent = (float)hitCount / (float)samplePoints.Count;
        if (percent >= requiredPercentage)
        {
            yaCompletada = true;
            OnLetterCompleted?.Invoke();

            if (managerLetra != null)
                managerLetra.CompletarLetra(indiceLetra);
        }
    }

    class Vector2Comparer : IEqualityComparer<Vector2>
    {
        float eps;
        public Vector2Comparer(float eps) { this.eps = eps; }
        public bool Equals(Vector2 a, Vector2 b) { return Vector2.SqrMagnitude(a - b) <= eps * eps; }
        public int GetHashCode(Vector2 v) { return 0; }
    }

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


    public void ReintentarTrazo()
    {
        LineRenderer[] lineas = GetComponentsInChildren<LineRenderer>(true);

        foreach (LineRenderer linea in lineas)
        {
            if(linea.transform.name == "LinePrefab")
                continue;

            if(linea == lr)
            {
                linea.positionCount = 0;
                continue;
            }

            Destroy(linea.gameObject);
        }

        linePrefab = transform.Find("LinePrefab");

        currentLine = null;
        currentStroke.Clear();
        isTracing = false;
        hitCount = 0;

        for (int i = 0; i < sampleHit.Length; i++)
            sampleHit[i] = false;
    }

    IEnumerator LimiteIntentos()
    {
        mostrandoReintento = true;
        isTracing = false;

        MensajeFallo.transform.SetAsLastSibling();
        MensajeFallo.SetActive(true);

        yield return new WaitForSeconds(1.8f);

        MensajeFallo.SetActive(false);

        managerLetra.RegresarPanel();
    }

    public void ReiniciarIntentos()
    {
        fallos = 0;
        limiteActivado = false;
        mostrandoReintento = false;
    }
}