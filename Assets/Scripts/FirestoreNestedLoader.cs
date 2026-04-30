using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;

public class FirestoreNestedLoader : MonoBehaviour
{
    public static FirestoreNestedLoader Instance;
    private FirebaseFirestore db;

    private void Awake()
    {
        Instance = this;
    }

    public void LoadWordsByAge(string edadBuscada, System.Action<List<WordEntry>> onLoaded)
    {
        StartCoroutine(LoadWordsRoutine(edadBuscada, onLoaded));
    }

    private IEnumerator LoadWordsRoutine(string edadBuscada, System.Action<List<WordEntry>> onLoaded)
    {
        List<WordEntry> words = new List<WordEntry>();

        // Esperar a que Firebase realmente esté listo
        float timeout = 15f;
        float timer = 0f;

        while (!FirebaseInitializer.IsReady && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!FirebaseInitializer.IsReady)
        {
            Debug.LogError("Firebase no se inicializó a tiempo");
            onLoaded?.Invoke(words);
            yield break;
        }

        // SOLO aquí obtener Firestore
        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }

        Debug.Log("Iniciando consulta Firestore...");

        var task = db.Collection("diccionario").Document("maestro").GetSnapshotAsync();

        timer = 0f;
        while (!task.IsCompleted && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!task.IsCompleted)
        {
            Debug.LogError("Timeout consultando Firestore");
            onLoaded?.Invoke(words);
            yield break;
        }

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Error leyendo Firestore: " + task.Exception);
            onLoaded?.Invoke(words);
            yield break;
        }

        DocumentSnapshot doc = task.Result;
        if (!doc.Exists)
        {
            Debug.LogError("No existe diccionario/maestro");
            onLoaded?.Invoke(words);
            yield break;
        }

        Dictionary<string, object> root = doc.ToDictionary();

        if (!root.ContainsKey(edadBuscada))
        {
            Debug.LogError("No existe la edad buscada: " + edadBuscada);
            onLoaded?.Invoke(words);
            yield break;
        }

        var ageMap = root[edadBuscada] as Dictionary<string, object>;
        if (ageMap == null)
        {
            Debug.LogError("La edad no es un Dictionary válido");
            onLoaded?.Invoke(words);
            yield break;
        }

        foreach (var tipoPair in ageMap)
        {
            string tipo = tipoPair.Key;
            var tipoMap = tipoPair.Value as Dictionary<string, object>;
            if (tipoMap == null) continue;

            foreach (var difPair in tipoMap)
            {
                string dificultad = difPair.Key;
                var palabrasList = difPair.Value as List<object>;
                if (palabrasList == null) continue;

                foreach (var item in palabrasList)
                {
                    var palabraMap = item as Dictionary<string, object>;
                    if (palabraMap == null) continue;

                    WordEntry w = new WordEntry();
                    w.edad = edadBuscada;
                    w.tipo = tipo;
                    w.dificultad = dificultad;
                    w.texto = palabraMap.ContainsKey("texto") ? palabraMap["texto"].ToString() : "";
                    w.textoLimpio = palabraMap.ContainsKey("texto_limpio") ? palabraMap["texto_limpio"].ToString() : "";
                    w.silabas = new List<string>();

                    if (palabraMap.ContainsKey("silabas"))
                    {
                        var silabasList = palabraMap["silabas"] as List<object>;
                        if (silabasList != null)
                        {
                            foreach (var s in silabasList)
                                w.silabas.Add(s.ToString());
                        }
                    }

                    if (!string.IsNullOrEmpty(w.textoLimpio))
                        words.Add(w);
                }
            }
        }

        Debug.Log("Palabras cargadas final: " + words.Count);
        onLoaded?.Invoke(words);
    }
}