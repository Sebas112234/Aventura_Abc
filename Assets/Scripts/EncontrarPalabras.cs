using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Firebase.Firestore;
using Firebase.Extensions;

public class WordFinderManager : MonoBehaviour
{
    [Header("UI - Teclado (7 botones)")]
    public TextMeshProUGUI[] letterButtonsText; 
    public TextMeshProUGUI currentWordText;
    
    [Header("UI - Estado y Mensajes")]
    public TextMeshProUGUI foundWordsListText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI feedbackText;

    private List<string> dbWords = new List<string>();
    private List<string> validWordsInRound = new List<string>();
    private List<string> foundWords = new List<string>();
    private string currentInput = "";
    private float timeLeft = 300f; 
    private bool isGameActive = false;
    private int targetCountThisRound = 10;

    void Start() {
        Debug.Log("<color=yellow>1. Iniciando Juego...</color>");
        feedbackText.text = "Conectando...";
        
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == Firebase.DependencyStatus.Available) {
                FetchData();
            } else {
                feedbackText.text = "Error Dependencias";
            }
        });
    }

    void FetchData() {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        // IMPORTANTE: Verifica que tu colección se llame "diccionario" y el documento "maestro"
        db.Collection("diccionario").Document("maestro").GetSnapshotAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                Debug.LogError("Error Firebase: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists) {
                // USAMOS EL NUEVO EXPLORADOR RECURSIVO
                dbWords.Clear();
                ExplorarDiccionario(snapshot.ToDictionary());
                
                Debug.Log("<color=cyan>TOTAL PALABRAS CARGADAS: </color>" + dbWords.Count);
                
                if (dbWords.Count > 0) StartNewGame();
                else feedbackText.text = "BD vacía o mal estructurada";
            }
        });
    }

    // ESTA FUNCIÓN BUSCA PALABRAS EN CUALQUER NIVEL DEL DOCUMENTO
    void ExplorarDiccionario(IDictionary<string, object> nodo) {
        foreach (var entrada in nodo) {
            if (entrada.Value is IDictionary<string, object> subNodo) {
                ExplorarDiccionario(subNodo); // Sigue bajando niveles
            } 
            else if (entrada.Value is List<object> lista) {
                foreach (var item in lista) {
                    if (item is IDictionary<string, object> datosPalabra) {
                        string p = "";
                        if (datosPalabra.ContainsKey("texto_limpio")) p = datosPalabra["texto_limpio"].ToString();
                        else if (datosPalabra.ContainsKey("texto")) p = datosPalabra["texto"].ToString();

                        if (!string.IsNullOrEmpty(p)) {
                            dbWords.Add(LimpiarCadena(p));
                        }
                    }
                }
            }
        }
    }

    void GenerateValidLetterSet() {
    string vocales = "AEIOU";
    string consonantes = "RSTNLMPDBCFGHJK";
    bool encontrado = false;
    int intentos = 0;
    targetCountThisRound = 10; 

    while (!encontrado && intentos < 2000) {
        intentos++;
        
        List<char> set = vocales.OrderBy(x => Random.value).Take(3).ToList();
        var resto = consonantes.OrderBy(x => Random.value).Take(4).ToList();
        set.AddRange(resto);
        set = set.Distinct().ToList();

        while(set.Count < 7) {
            char extra = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ"[Random.Range(0, 27)];
            if(!set.Contains(extra)) set.Add(extra);
        }

        validWordsInRound = dbWords.Where(w => IsWordFormable(w, set)).ToList();

        if (intentos > 1500) targetCountThisRound = 5;
        else if (intentos > 800) targetCountThisRound = 8;

        if (validWordsInRound.Count >= targetCountThisRound) {
            encontrado = true;
            for (int i = 0; i < 7; i++) {
                letterButtonsText[i].text = set[i].ToString();
            }

            // --- AQUÍ ESTÁ EL LOG QUE NECESITAS ---
            string chuletero = string.Join(", ", validWordsInRound);
            Debug.Log("<color=cyan><b>¡TRAMPA ACTIVADA! Las palabras son: </b></color>" + chuletero);
            
            feedbackText.text = $"¡Encuentra {validWordsInRound.Count} palabras!";
        }
    }
}

    bool IsWordFormable(string word, List<char> letters) {
        if (word.Length < 3) return false; // Ignorar palabras de 1 o 2 letras
        foreach (char c in word) {
            if (!letters.Contains(c)) return false;
        }
        return true;
    }

    string LimpiarCadena(string s) {
        string normalizedString = s.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();
        foreach (char c in normalizedString) {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToUpper().Trim();
    }

    public void AddLetter(int index) {
        if (!isGameActive) return;
        currentInput += letterButtonsText[index].text;
        currentWordText.text = "Palabra: " + currentInput;
    }

    // Función para el botón "Borrar"
public void DeleteLetter() {
    if (!isGameActive) return; // Si ya ganó o perdió, no borramos nada
    
    if (currentInput.Length > 0) {
        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        currentWordText.text = "Escribe: " + currentInput;
    }
}

// Función para el botón "Reintentar"
public void StartNewGame() {
    // 1. Detenemos cualquier proceso previo
    isGameActive = false; 
    
    // 2. Limpiamos listas y variables
    foundWords.Clear();
    currentInput = "";
    
    // 3. Limpiamos la UI
    foundWordsListText.text = "";
    currentWordText.text = "Escribe:";
    feedbackText.text = "¡Busca las palabras!";
    feedbackText.color = Color.white;
    timeLeft = 300f; // Reiniciamos el reloj
    
    // 4. Generamos el nuevo set de letras y activamos
    GenerateValidLetterSet();
    isGameActive = true;
    
    Debug.Log("<color=green>Juego Reiniciado</color>");
}

    public void ConfirmWord() {
    if (!isGameActive) return;
    string intento = LimpiarCadena(currentInput);

    if (validWordsInRound.Contains(intento) && !foundWords.Contains(intento)) {
        foundWords.Add(intento);
        foundWordsListText.text += intento + "  ";
        
        // Ahora comparamos contra la lista válida de la ronda, no contra un número fijo
        feedbackText.text = $"¡Bien! {foundWords.Count}/{validWordsInRound.Count}";
        feedbackText.color = Color.green;
        
        if (foundWords.Count >= validWordsInRound.Count) EndGame(true);
    } else {
        feedbackText.text = "No válida";
        feedbackText.color = Color.red;
    }
    currentInput = "";
    currentWordText.text = "Escribe:";
}

    void Update() {
        if (!isGameActive) return;
        if (timeLeft > 0) {
            timeLeft -= Time.deltaTime;
            timerText.text = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(timeLeft / 60), Mathf.FloorToInt(timeLeft % 60));
        } else EndGame(false);
    }

    void EndGame(bool success) {
        isGameActive = false;
        feedbackText.text = success ? "¡GANASTE!" : "TIEMPO AGOTADO";
        feedbackText.color = success ? Color.green : Color.red;
    }
}