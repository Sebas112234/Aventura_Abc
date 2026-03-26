using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    
    [Header("Botones de Control")]
    public Button botonSiguiente;
    public Button botonValidar; // Asigna el nuevo botón aquí

    [Header("Prefabs UI")]
    public GameObject prefabSlotSuperior; 
    public GameObject prefabBotonInferior; 

    private FirebaseFirestore db; 
    private List<PalabraCsharp> listaPalabrasCargadas = new List<PalabraCsharp>();
    private PalabraCsharp palabraCorrectaActual;
    private int indiceSilabaOculta;
    private GameObject slotVacioGo;
    
    // Variables para control de validación
    private GameObject objetoSilabaEnSlot; 
    private string textoSilabaEnSlot = "";

    void Start() {
        // Configuración inicial de botones
        botonSiguiente.onClick.AddListener(CargarSiguientePalabra);
        botonValidar.onClick.AddListener(ValidarRespuestaManual);
        
        botonSiguiente.interactable = false;
        botonValidar.interactable = false;
        textPalabraPista.text = "Cargando...";

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available) {
                db = FirebaseFirestore.DefaultInstance;
                DescargarTodoElDiccionario();
            } else {
                textPalabraPista.text = "Error Firebase";
                Debug.LogError("No se pudieron resolver las dependencias de Firebase.");
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
                CargarSiguientePalabra();
            }
        });
    }

    public void CargarSiguientePalabra() {
        if (listaPalabrasCargadas.Count == 0) return;
        
        LimpiarContenedores();

        // Reset de estado
        objetoSilabaEnSlot = null;
        textoSilabaEnSlot = "";
        botonValidar.interactable = false;
        botonSiguiente.interactable = false;

        // Selección aleatoria
        palabraCorrectaActual = listaPalabrasCargadas[Random.Range(0, listaPalabrasCargadas.Count)];
        PalabraCsharp basura = listaPalabrasCargadas[Random.Range(0, listaPalabrasCargadas.Count)];
        
        textPalabraPista.text = palabraCorrectaActual.Texto.ToUpper();
        indiceSilabaOculta = Random.Range(0, palabraCorrectaActual.Silabas.Count);

        // Fila Superior
        for (int i = 0; i < palabraCorrectaActual.Silabas.Count; i++) {
            GameObject go = Instantiate(prefabSlotSuperior, contenedorFilaSuperior);
            var txt = go.GetComponentInChildren<TMP_Text>();
            txt.color = Color.black; // Reset color

            if (i == indiceSilabaOculta) {
                txt.text = "_";
                slotVacioGo = go;
                go.GetComponent<DropSlot>().OnItemDroppedEvent += AlSoltarSilabaEnSlot;
            } else {
                txt.text = palabraCorrectaActual.Silabas[i];
            }
        }

        // Fila Inferior (Opciones)
        List<string> opciones = new List<string> { palabraCorrectaActual.Silabas[indiceSilabaOculta] };
        // Añadimos un par de sílabas de otra palabra para distraer
        opciones.AddRange(basura.Silabas.Take(2)); 
        opciones = opciones.OrderBy(x => Random.value).ToList();

        foreach (string s in opciones) {
            GameObject bGo = Instantiate(prefabBotonInferior, contenedorFilaInferior);
            bGo.GetComponentInChildren<TMP_Text>().text = s;
        }
    }

    void AlSoltarSilabaEnSlot(GameObject dropped) {
        // Si ya había una sílaba en el slot, devolvemos la anterior a su posición original
        if (objetoSilabaEnSlot != null) {
            objetoSilabaEnSlot.SetActive(true);
            objetoSilabaEnSlot.GetComponent<DragHandler>().RegresarAPosicionOriginal();
        }

        objetoSilabaEnSlot = dropped;
        textoSilabaEnSlot = dropped.GetComponentInChildren<TMP_Text>().text;
        
        // Actualizar visualmente el slot vacío
        slotVacioGo.GetComponentInChildren<TMP_Text>().text = textoSilabaEnSlot;
        slotVacioGo.GetComponentInChildren<TMP_Text>().color = Color.blue;

        // Desactivamos la ficha de abajo para que parezca que "subió"
        dropped.SetActive(false);
        
        botonValidar.interactable = true;
    }

    public void ValidarRespuestaManual() {
        string silabaCorrecta = palabraCorrectaActual.Silabas[indiceSilabaOculta];

        if (textoSilabaEnSlot == silabaCorrecta) {
            // CORRECTO
            slotVacioGo.GetComponentInChildren<TMP_Text>().color = Color.green;
            botonValidar.interactable = false;
            botonSiguiente.interactable = true;
            CargarSiguientePalabra();
        } else {
            // INCORRECTO
            Debug.Log("Sílaba incorrecta, regresando...");
            RegresarSilabaPorError();
        }
    }

    void RegresarSilabaPorError() {
        if (objetoSilabaEnSlot != null) {
            objetoSilabaEnSlot.SetActive(true);
            objetoSilabaEnSlot.GetComponent<DragHandler>().RegresarAPosicionOriginal();
        }

        slotVacioGo.GetComponentInChildren<TMP_Text>().text = "_";
        slotVacioGo.GetComponentInChildren<TMP_Text>().color = Color.black;
        
        objetoSilabaEnSlot = null;
        textoSilabaEnSlot = "";
        botonValidar.interactable = false;
    }

    void LimpiarContenedores() {
        foreach (Transform child in contenedorFilaSuperior) Destroy(child.gameObject);
        foreach (Transform child in contenedorFilaInferior) Destroy(child.gameObject);
    }
}