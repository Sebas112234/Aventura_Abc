using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ListenWordsUI : MonoBehaviour
{
    public TMP_Text feedbackText;
    public TMP_Text roundText;
    public Button[] optionButtons;

    private List<string> palabras = new List<string>()
    {
        "GATO", "PERRO", "ELEFANTE", "LEON", "TIGRE"
    };

    private int currentRound = 0;
    private int maxRounds = 10;
    private string correctAnswer;

    void Start()
    {
        NextRound();
    }

    void NextRound()
    {
        feedbackText.text = "";
        currentRound++;

        if (currentRound > maxRounds)
        {
            Debug.Log("Juego terminado");
            return;
        }

        roundText.text = currentRound + " / " + maxRounds;

        // elegir correcta
        correctAnswer = palabras[Random.Range(0, palabras.Count)];

        // generar opciones
        List<string> opciones = new List<string>();
        opciones.Add(correctAnswer);

        while (opciones.Count < 4)
        {
            string random = palabras[Random.Range(0, palabras.Count)];
            if (!opciones.Contains(random))
                opciones.Add(random);
        }

        // mezclar
        for (int i = 0; i < opciones.Count; i++)
        {
            string temp = opciones[i];
            int randomIndex = Random.Range(0, opciones.Count);
            opciones[i] = opciones[randomIndex];
            opciones[randomIndex] = temp;
        }

        // asignar botones
        for (int i = 0; i < optionButtons.Length; i++)
        {
            string palabra = opciones[i];
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = palabra;

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => CheckAnswer(palabra));
        }
    }

    void CheckAnswer(string answer)
    {
        if (answer == correctAnswer)
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