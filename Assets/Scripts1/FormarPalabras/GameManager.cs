using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necesario para TextMeshPro
using System.Collections.Generic;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Linq;

public class PalabraCsharp {
    public string Texto { get; set; }
    public List<string> Silabas { get; set; }
}

public class GameManager : MonoBehaviour {
    [Header("Conexión Firestore")]
    public string collectionName = "diccionario";
    public string documentId = "maestro";
    public string rangoEdad = "2-4";

    [Header("UI - Referencias")]
    public TMP_Text textPalabraPista; 
    public Transform contenedorFilaSuperior; 
    public Transform contenedorFilaInferior; 
    public Button botonSiguiente;

    [Header("Prefabs UI")]
    public GameObject prefabSlotSuperior; 
    public GameObject prefabBotonInferior; 

    private FirebaseFirestore db; 
    private List<PalabraCsharp> listaPalabrasCargadas = new List<PalabraCsharp>();
    private PalabraCsharp palabraCorrectaActual;
    private int indiceSilabaOculta;
    private GameObject slotVacioGo;

    void Start() {
        botonSiguiente.onClick.AddListener(CargarSiguientePalabra);
        botonSiguiente.interactable = false;
        textPalabraPista.text = "Cargando...";

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available) {
                db = FirebaseFirestore.DefaultInstance;
                DescargarTodoElDiccionario();
            } else {
                textPalabraPista.text = "Error Firebase";
            }
        });
    }

    void DescargarTodoElDiccionario() {
        db.Collection(collectionName).Document(documentId).GetSnapshotAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) return;
            DocumentSnapshot snapshot = task.Result;
            if (!snapshot.Exists) return;

            Dictionary<string, object> rootData = snapshot.ToDictionary();
            if (rootData.ContainsKey(rangoEdad)) {
                var ageGroupData = rootData[rangoEdad] as Dictionary<string, object>;
                foreach (var categoria in ageGroupData) {
                    var categoryData = categoria.Value as Dictionary<string, object>;
                    foreach (var dificultad in categoryData) {
                        var palabrasList = dificultad.Value as List<object>;
                        if (palabrasList != null) {
                            foreach (object obj in palabrasList) {
                                var wordData = obj as Dictionary<string, object>;
                                var nueva = new PalabraCsharp {
                                    Texto = wordData["texto_limpio"].ToString(),
                                    Silabas = (wordData["silabas"] as List<object>).Select(s => s.ToString()).ToList()
                                };
                                listaPalabrasCargadas.Add(nueva);
                            }
                        }
                    }
                }
                botonSiguiente.interactable = true;
                CargarSiguientePalabra();
            }
        });
    }

    public void CargarSiguientePalabra() {
        if (listaPalabrasCargadas.Count == 0) return;
        LimpiarContenedores();

        palabraCorrectaActual = listaPalabrasCargadas[Random.Range(0, listaPalabrasCargadas.Count)];
        PalabraCsharp basura = listaPalabrasCargadas[Random.Range(0, listaPalabrasCargadas.Count)];
        
        textPalabraPista.text = palabraCorrectaActual.Texto.ToUpper();
        indiceSilabaOculta = Random.Range(0, palabraCorrectaActual.Silabas.Count);

        // Fila Superior
        for (int i = 0; i < palabraCorrectaActual.Silabas.Count; i++) {
            GameObject go = Instantiate(prefabSlotSuperior, contenedorFilaSuperior);
            var txt = go.GetComponentInChildren<TMP_Text>();
            if (i == indiceSilabaOculta) {
                txt.text = "_";
                slotVacioGo = go;
                go.GetComponent<DropSlot>().OnItemDroppedEvent += AlSoltarSilaba;
            } else {
                txt.text = palabraCorrectaActual.Silabas[i];
            }
        }

        // Fila Inferior
        List<string> opciones = new List<string> { palabraCorrectaActual.Silabas[indiceSilabaOculta] };
        opciones.AddRange(basura.Silabas);
        opciones = opciones.OrderBy(x => Random.value).ToList();

        foreach (string s in opciones) {
            GameObject bGo = Instantiate(prefabBotonInferior, contenedorFilaInferior);
            bGo.GetComponentInChildren<TMP_Text>().text = s;
        }
    }

    void AlSoltarSilaba(GameObject dropped) {
        if (dropped.GetComponentInChildren<TMP_Text>().text == palabraCorrectaActual.Silabas[indiceSilabaOculta]) {
            slotVacioGo.GetComponentInChildren<TMP_Text>().text = palabraCorrectaActual.Silabas[indiceSilabaOculta];
            dropped.SetActive(false);
        } else {
            dropped.GetComponent<DragHandler>().RegresarAPosicionOriginal();
        }
    }

    void LimpiarContenedores() {
        foreach (Transform child in contenedorFilaSuperior) Destroy(child.gameObject);
        foreach (Transform child in contenedorFilaInferior) Destroy(child.gameObject);
    }
}