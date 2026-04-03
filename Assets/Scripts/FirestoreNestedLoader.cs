using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class FirestoreNestedLoader : MonoBehaviour
{
    public static FirestoreNestedLoader Instance;
    private FirebaseFirestore db;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void LoadWordsByAge(string edadBuscada, System.Action<List<WordEntry>> onLoaded)
    {
        db.Collection("diccionario").Document("maestro").GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                List<WordEntry> words = new List<WordEntry>();

                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Error leyendo Firestore: " + task.Exception);
                    onLoaded?.Invoke(words);
                    return;
                }

                DocumentSnapshot doc = task.Result;
                if (!doc.Exists)
                {
                    Debug.LogError("No existe diccionario/maestro");
                    onLoaded?.Invoke(words);
                    return;
                }

                Dictionary<string, object> root = doc.ToDictionary();

                if (!root.ContainsKey(edadBuscada))
                {
                    Debug.LogError("No existe la edad buscada: " + edadBuscada);
                    onLoaded?.Invoke(words);
                    return;
                }

                var ageMap = root[edadBuscada] as Dictionary<string, object>;
                if (ageMap == null)
                {
                    Debug.LogError("La edad no es un Dictionary válido");
                    onLoaded?.Invoke(words);
                    return;
                }

                foreach (var tipoPair in ageMap)
                {
                    string tipo = tipoPair.Key;
                    var tipoMap = tipoPair.Value as Dictionary<string, object>;
                    if (tipoMap == null) continue;

                    foreach (var difPair in tipoMap)
                    {
                        string dificultad = difPair.Key;

                        // AQUÍ ESTÁ EL CAMBIO IMPORTANTE:
                        var palabrasList = difPair.Value as List<object>;
                        if (palabrasList == null)
                        {
                            Debug.LogWarning("La dificultad " + dificultad + " no es una lista");
                            continue;
                        }

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
                                    {
                                        w.silabas.Add(s.ToString());
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(w.textoLimpio))
                            {
                                words.Add(w);
                                Debug.Log("Palabra cargada: " + w.textoLimpio);
                            }
                        }
                    }
                }

                Debug.Log("Palabras cargadas final: " + words.Count);
                onLoaded?.Invoke(words);
            });
    }
}