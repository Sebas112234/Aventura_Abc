using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
        feedbackText.text = "Inicializando Firebase...";
        roundText.text = "0 / " + maxRounds;
        StartCoroutine(WaitForFirebaseAndLoad());
    }

    IEnumerator WaitForFirebaseAndLoad()
    {
        float timeout = 15f;
        float timer = 0f;

        while (!FirebaseInitializer.IsReady && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!FirebaseInitializer.IsReady)
        {
            feedbackText.text = "Firebase no se inicializó";
            yield break;
        }

        feedbackText.text = "Cargando palabras...";
        FirestoreNestedLoader.Instance.LoadWordsByAge(edadActual, OnWordsLoaded);
    }

    void OnWordsLoaded(List<WordEntry> loaded)
    {
        palabras = loaded;

        if (palabras == null)
        {
            feedbackText.text = "Error cargando palabras";
            return;
        }

        if (palabras.Count == 0)
        {
            feedbackText.text = "Consulta vacía o timeout";
            return;
        }

        if (palabras.Count < 4)
        {
            feedbackText.text = "Faltan palabras (mínimo 4)";
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

        if (listenLabel != null)
            listenLabel.text = "Escuchar palabra";
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