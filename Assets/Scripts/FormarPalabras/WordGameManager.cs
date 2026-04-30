using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Linq;
using TMPro; // <--- Si esto sale en rojo en tu editor, necesitas importar TMP en Unity

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
    public TextMeshProUGUI feedbackText;    // Error CS0246 suele apuntar aquí
    public TextMeshProUGUI roundCounterText;
    public GameObject completedPanel;

    private FirebaseFirestore db;
    private List<WordData> allWords = new List<WordData>();
    private WordData targetWord;
    private string missingSyllable;
    private int wordsPlayed = 0;
    private const int MAX_WORDS_PER_ROUND = 10;

    void Start() {
        // Inicialización segura de Firebase
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                db = FirebaseFirestore.DefaultInstance;
                completedPanel.SetActive(false);
                feedbackText.text = "Conectado. Cargando datos...";
                FetchWordsFromFirestore();
            } else {
                Debug.LogError("No se pudieron resolver las dependencias de Firebase: " + dependencyStatus);
                feedbackText.text = "Error de dependencias Firebase";
            }
        });
    }

    void FetchWordsFromFirestore() {
        db.Collection("diccionario").Document("maestro").GetSnapshotAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                feedbackText.text = "Error de red";
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists) {
                MapearDiccionario(snapshot.ToDictionary());
                if (allWords.Count > 0) StartNewRound();
                else feedbackText.text = "Banco de palabras vacío";
            }
        });
    }

    void MapearDiccionario(IDictionary<string, object> root) {
        allWords.Clear();
        foreach (var ageGroup in root) {
            var categories = ageGroup.Value as IDictionary<string, object>;
            if (categories == null) continue;

            foreach (var category in categories) {
                var levels = category.Value as IDictionary<string, object>;
                if (levels == null) continue;

                foreach (var levelEntry in levels) {
                    var wordList = levelEntry.Value as List<object>;
                    if (wordList == null) continue;

                    foreach (var item in wordList) {
                        var wordMap = item as IDictionary<string, object>;
                        if (wordMap != null && wordMap.ContainsKey("silabas")) {
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
            return;
        }

        foreach (Transform child in targetWordContainer) Destroy(child.gameObject);
        foreach (Transform child in optionsContainer) Destroy(child.gameObject);
        
        targetWord = allWords[UnityEngine.Random.Range(0, allWords.Count)];
        WordData garbageWord = allWords[UnityEngine.Random.Range(0, allWords.Count)];

        int missingIndex = UnityEngine.Random.Range(0, targetWord.silabas.Count);
        missingSyllable = targetWord.silabas[missingIndex];

        for (int i = 0; i < targetWord.silabas.Count; i++) {
            GameObject go = Instantiate(syllablePrefab, targetWordContainer);
            var slot = go.GetComponent<SyllableSlot>();
            slot.Setup(i == missingIndex ? "-" : targetWord.silabas[i], i == missingIndex);
        }

        List<string> options = new List<string> { missingSyllable };
        options.AddRange(garbageWord.silabas);
        options = options.OrderBy(x => UnityEngine.Random.value).ToList();

        foreach (string s in options) {
            GameObject go = Instantiate(syllablePrefab, optionsContainer);
            go.GetComponent<DraggableSyllable>().Init(s, this);
        }

        wordsPlayed++;
        roundCounterText.text = $"Palabra {wordsPlayed}/{MAX_WORDS_PER_ROUND}";
    }

    public void OnSyllableDropped(string value, DraggableSyllable draggedScript) {
    if (value == missingSyllable) {
        // CORRECCIÓN PARA EL SOFTLOCK: Asegurarnos que el feedback sea positivo
        feedbackText.text = "¡Excelente!";
        feedbackText.color = Color.green;

        // Actualizar visualmente el hueco arriba
        foreach (Transform child in targetWordContainer) {
            var slot = child.GetComponent<SyllableSlot>();
            if (slot != null && slot.isPlaceholder) {
                slot.textMesh.text = value;
                slot.textMesh.color = Color.green;
            }
        }

        draggedScript.gameObject.SetActive(false);
        Invoke("StartNewRound", 1.2f);
    } else {
        // DETALLE 1: Texto rojo "Intenta otra vez"
        feedbackText.text = "Intenta otra vez";
        feedbackText.color = Color.red;

        // DETALLE 2: Evitar softlock llamando al reset del script de arrastre
        draggedScript.ReturnToStart();
    }
}

    public void SkipWord() => StartNewRound();
}