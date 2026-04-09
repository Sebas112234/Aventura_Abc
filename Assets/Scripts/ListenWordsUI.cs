using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ListenWordsUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text feedbackText;
    public TMP_Text roundText;
    public TMP_Text listenLabel;
    public Button[] optionButtons;

    [Header("TTS")]
    public AndroidTTS tts;

    [Header("Settings")]
    public string edadActual = "2-4"; 
    public int maxRounds = 10;

    private List<WordEntry> palabras = new List<WordEntry>();
    private int currentRound = 0;
    private WordEntry correctWord;

    private void Start()
    {
        feedbackText.text = "Cargando...";
        roundText.text = "0 / " + maxRounds;

        Invoke(nameof(BeginLoad), 1f);
    }

    void BeginLoad()
    {
        if (!FirebaseInitializer.IsReady)
        {
            feedbackText.text = "Firebase no listo";
            return;
        }

        FirestoreNestedLoader.Instance.LoadWordsByAge(edadActual, OnWordsLoaded);
    }

    void OnWordsLoaded(List<WordEntry> loaded)
    {
        palabras = loaded;

        if (palabras == null || palabras.Count < 4)
        {
            feedbackText.text = "No hay suficientes palabras";
            return;
        }

        feedbackText.text = "";
        NextRound();
    }

    void NextRound()
    {
        currentRound++;

        if (currentRound > maxRounds)
        {
            feedbackText.text = "Juego terminado";
            roundText.text = maxRounds + " / " + maxRounds;
            return;
        }

        roundText.text = currentRound + " / " + maxRounds;

        correctWord = palabras[Random.Range(0, palabras.Count)];

        List<WordEntry> opciones = new List<WordEntry>();
        opciones.Add(correctWord);

        while (opciones.Count < 4)
        {
            WordEntry random = palabras[Random.Range(0, palabras.Count)];
            if (!opciones.Any(x => x.textoLimpio == random.textoLimpio))
                opciones.Add(random);
        }

        opciones = opciones.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < optionButtons.Length; i++)
        {
            WordEntry selected = opciones[i];
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = selected.textoLimpio;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => CheckAnswer(selected));
        }

        //listenLabel.text = "Escuchar palabra";
    }

    public void OnListenPressed()
    {
        if (correctWord == null)
        {
            feedbackText.text = "No hay palabra cargada";
            return;
        }

#if UNITY_EDITOR
    feedbackText.text = "TTS solo funciona en Android";
    Debug.Log("Editor: TTS no se ejecuta aquí");
    return;
#endif

        if (tts == null)
        {
            feedbackText.text = "TTS no asignado";
            return;
        }

        if (!tts.IsReady)
        {
            feedbackText.text = "TTS aún no está listo";
            Debug.LogWarning("TTS aún no está listo al presionar botón");
            return;
        }

        feedbackText.text = "";
        tts.Speak(correctWord.texto);
    }

    void CheckAnswer(WordEntry answer)
    {
        if (answer.textoLimpio == correctWord.textoLimpio)
        {
            feedbackText.text = "¡Muy bien!";
            Invoke(nameof(NextRound), 1f);
        }
        else
        {
            feedbackText.text = "Inténtalo de nuevo";
        }
    }
}