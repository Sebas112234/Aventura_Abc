using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

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
    private int aciertos = 0;
    private int errores = 0;
    private string nombreJuego = "Encontrar Palabras";

    void Start() {
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
       
        db.Collection("diccionario").Document("maestro").GetSnapshotAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) return;

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists) {
                dbWords.Clear();
                ExplorarDiccionario(snapshot.ToDictionary());
                if (dbWords.Count > 0) StartNewGame();
                else feedbackText.text = "BD vacía";
            }
        });
    }

    void ExplorarDiccionario(IDictionary<string, object> nodo) {
        foreach (var entrada in nodo) {
            if (entrada.Value is IDictionary<string, object> subNodo) ExplorarDiccionario(subNodo);
            else if (entrada.Value is List<object> lista) {
                foreach (var item in lista) {
                    if (item is IDictionary<string, object> datosPalabra) {
                        string p = datosPalabra.ContainsKey("texto_limpio") ? datosPalabra["texto_limpio"].ToString() : 
                                  (datosPalabra.ContainsKey("texto") ? datosPalabra["texto"].ToString() : "");
                        if (!string.IsNullOrEmpty(p)) dbWords.Add(LimpiarCadena(p));
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

            if (validWordsInRound.Count >= 5) { 
                encontrado = true;
                for (int i = 0; i < 7; i++) letterButtonsText[i].text = set[i].ToString();
                feedbackText.text = $"¡Encuentra las {validWordsInRound.Count} palabras!";
            }
        }
    }

    bool IsWordFormable(string word, List<char> letters) {
        if (word.Length < 3) return false;
        foreach (char c in word) if (!letters.Contains(c)) return false;
        return true;
    }

    System.Collections.IEnumerator RegresoAutomaticoMenu() {
        yield return new WaitForSeconds(3.5f);
        Menu();
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

    public void DeleteLetter() {
        if (!isGameActive) return;
        if (currentInput.Length > 0) {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            currentWordText.text = "Escribe: " + currentInput;
        }
    }

    public void StartNewGame() {
        isGameActive = false; 
        foundWords.Clear();
        aciertos = 0;
        errores = 0;

        currentInput = "";
        foundWordsListText.text = "";
        currentWordText.text = "Escribe:";
        feedbackText.text = "¡Busca las palabras!";
        feedbackText.color = Color.white;
        timeLeft = 300f; 
        
        GenerateValidLetterSet();
        isGameActive = true;
    }

    public void ConfirmWord() {
        if (!isGameActive || string.IsNullOrEmpty(currentInput)) return;
        string intento = LimpiarCadena(currentInput);

        if (validWordsInRound.Contains(intento) && !foundWords.Contains(intento)) {
            aciertos++;
            foundWords.Add(intento);
            foundWordsListText.text += intento + "  ";
            feedbackText.text = $"¡Bien! {foundWords.Count}/{validWordsInRound.Count}";
            feedbackText.color = Color.green;
            
            if (foundWords.Count >= validWordsInRound.Count) EndGame(true);
        } else {
            errores++;
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
        StartCoroutine(RegresoAutomaticoMenu());
    }

    public void Menu() {
        if (aciertos > 0 || errores > 0) {
            int rExito = (aciertos > errores) ? 1 : 0;
            int rFalla = (aciertos > errores) ? 0 : 1;
            HistorialManager.GuardarOActualizarProgreso(nombreJuego, aciertos, errores, rExito, rFalla);
        }

        int edad = HistorialManager.ObtenerEdadGuardada();
        if (edad == 1) SceneManager.LoadScene("03_Levels_2_4");
        else SceneManager.LoadScene("04_Levels_5_7");
    }
}