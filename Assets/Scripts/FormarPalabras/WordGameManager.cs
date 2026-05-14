using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

[Serializable]
public class WordData {
    public string texto;
    public List<string> silabas;
    public string categoria;

    public WordData(string txt, List<object> sil, string cat) {
        texto = txt;
        categoria = cat;
        silabas = new List<string>();
        if (sil != null) {
            foreach (var s in sil) silabas.Add(s.ToString());
        }
    }
}

public class WordGameManager : MonoBehaviour {
    [Header("Configuración de UI")]
    public Transform targetWordContainer; 
    public Transform optionsContainer;    
    public GameObject syllablePrefab;     
    public TextMeshProUGUI feedbackText;    
    public TextMeshProUGUI roundCounterText;
    public GameObject completedPanel;

    private FirebaseFirestore db;
    private List<WordData> allWords = new List<WordData>();
    private WordData targetWord;
    private string missingSyllable;
    private int wordsPlayed = 0;
    private const int MAX_WORDS_PER_ROUND = 10;
    private string currentPlacedSyllable = ""; 
    private DraggableSyllable activeDraggedObject;
    private int aciertos = 0;
    private int errores = 0;
    private string nombreJuego = "Formar Palabras";

    void Start() {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == Firebase.DependencyStatus.Available) {
                db = FirebaseFirestore.DefaultInstance;
                completedPanel.SetActive(false);
                feedbackText.text = "Cargando palabras...";
                
                aciertos = 0;
                errores = 0;

                FetchWordsFromFirestore();
            } else {
                feedbackText.text = "Error Firebase";
            }
        });
    }

    void FetchWordsFromFirestore() {
        db.Collection("diccionario").Document("maestro").GetSnapshotAsync().ContinueWithOnMainThread(task => {
            if (snapshotExists(task)) {
                MapearDiccionario(task.Result.ToDictionary());
                if (allWords.Count > 0) StartNewRound();
            }
        });
    }

    bool snapshotExists(System.Threading.Tasks.Task<DocumentSnapshot> task) {
        return !task.IsFaulted && task.Result.Exists;
    }

    void MapearDiccionario(IDictionary<string, object> root) {
        allWords.Clear();
        foreach (var ageGroup in root) {
            if (!(ageGroup.Value is IDictionary<string, object> categories)) continue;
            foreach (var category in categories) {
                if (!(category.Value is IDictionary<string, object> levels)) continue;
                foreach (var levelEntry in levels) {
                    if (!(levelEntry.Value is List<object> wordList)) continue;
                    foreach (var item in wordList) {
                        if (item is IDictionary<string, object> wordMap && wordMap.ContainsKey("silabas")) {
                            allWords.Add(new WordData(
                                wordMap.ContainsKey("texto") ? wordMap["texto"].ToString() : "Sin nombre", 
                                wordMap["silabas"] as List<object>, 
                                category.Key
                            ));
                        }
                    }
                }
            }
        }
    }

    public void StartNewRound() {
        if (wordsPlayed >= MAX_WORDS_PER_ROUND) {
            completedPanel.SetActive(true);
            StartCoroutine(RegresoAutomaticoMenu());
            return;
        }

        currentPlacedSyllable = "";
        activeDraggedObject = null;

        foreach (Transform child in targetWordContainer) Destroy(child.gameObject);
        foreach (Transform child in optionsContainer) Destroy(child.gameObject);
        
        targetWord = allWords[UnityEngine.Random.Range(0, allWords.Count)];
        int missingIndex = UnityEngine.Random.Range(0, targetWord.silabas.Count);
        missingSyllable = targetWord.silabas[missingIndex];

        for (int i = 0; i < targetWord.silabas.Count; i++) {
            GameObject go = Instantiate(syllablePrefab, targetWordContainer);
            go.GetComponent<SyllableSlot>().Setup(i == missingIndex ? "-" : targetWord.silabas[i], i == missingIndex);
        }

        List<string> options = new List<string> { missingSyllable };
        while(options.Count < 3) {
            string randomSil = allWords[UnityEngine.Random.Range(0, allWords.Count)].silabas[0];
            if(!options.Contains(randomSil)) options.Add(randomSil);
        }
        options = options.OrderBy(x => UnityEngine.Random.value).ToList();

        foreach (string s in options) {
            GameObject go = Instantiate(syllablePrefab, optionsContainer);
            go.GetComponent<DraggableSyllable>().Init(s, this);
        }

        wordsPlayed++;
        roundCounterText.text = $"Palabra {wordsPlayed}/{MAX_WORDS_PER_ROUND}";
        feedbackText.text = "Arrastra la sílaba faltante";
        feedbackText.color = Color.white;
    }

    public void OnSyllableDropped(string value, DraggableSyllable draggedScript) {
        if (activeDraggedObject != null) {
            activeDraggedObject.gameObject.SetActive(true);
            activeDraggedObject.ReturnToStart();
        }

        currentPlacedSyllable = value;
        activeDraggedObject = draggedScript;

        foreach (Transform child in targetWordContainer) {
            var slot = child.GetComponent<SyllableSlot>();
            if (slot != null && slot.isPlaceholder) {
                slot.textMesh.text = value;
                slot.textMesh.color = new Color(0.2f, 0.5f, 1f); 
            }
        }

        draggedScript.gameObject.SetActive(false);
        feedbackText.text = "¿Es correcta?";
    }

    public void ValidarPalabra() {
        if (string.IsNullOrEmpty(currentPlacedSyllable)) {
            feedbackText.text = "Primero coloca una sílaba";
            return;
        }

        if (currentPlacedSyllable == missingSyllable) {
            aciertos++;
            feedbackText.text = "¡Muy bien!";
            feedbackText.color = Color.green;
            Invoke("StartNewRound", 1.2f);
        } else {
            errores++;
            feedbackText.text = "Casi, intenta de nuevo";
            feedbackText.color = Color.red;
            ResetPlaceholder(); 
        }
    }

    public void ResetPlaceholder() {
        if (activeDraggedObject != null) {
            activeDraggedObject.gameObject.SetActive(true);
            activeDraggedObject.ReturnToStart();
        }

        foreach (Transform child in targetWordContainer) {
            var slot = child.GetComponent<SyllableSlot>();
            if (slot != null && slot.isPlaceholder) {
                slot.textMesh.text = "-";
                slot.textMesh.color = Color.black;
            }
        }
        currentPlacedSyllable = "";
        activeDraggedObject = null;
    }

    System.Collections.IEnumerator RegresoAutomaticoMenu()
    {
        yield return new WaitForSeconds(3.5f);
        Menu();
    }

    public void SkipWord() => StartNewRound();
    
    public void Menu() {
        if (aciertos > 0 || errores > 0) {
            int rExito = (aciertos > errores) ? 1 : 0;
            int rFalla = (aciertos > errores) ? 0 : 1;
            HistorialManager.GuardarOActualizarProgreso(nombreJuego, aciertos, errores, rExito, rFalla);
        }

        int edad = HistorialManager.ObtenerEdadGuardada();
        SceneManager.LoadScene(edad == 1 ? "03_Levels_2_4" : "04_Levels_5_7");
    }
}