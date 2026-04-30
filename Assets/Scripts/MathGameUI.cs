using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum MathLevel
{
    Easy,       // 1–5
    Medium,     // 1–10
    Hard        // hasta 2 dígitos
}

public class MathGameUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text problemText;
    public TMP_Text operatorIcon;
    public TMP_Text feedbackText;
    public Button btnNext;

    public Transform groupA;
    public Transform groupB;

    public Button[] optionButtons;

    [Header("Prefabs")]
    public GameObject objectPrefab;

    [Header("Sprites")]
    public List<Sprite> objectSprites;

    [Header("Settings")]
    public MathLevel level = MathLevel.Easy;

    int a;
    int b;
    int result;
    bool isAddition;

    void Start()
    {
        GenerateProblem();
    }
    

    void GenerateProblem()
    {
        ClearObjects();
        feedbackText.text = "";

        int max = 5;

        if (level == MathLevel.Medium) max = 10;
        if (level == MathLevel.Hard) max = 20;

        a = Random.Range(1, max + 1);
        b = Random.Range(1, max + 1);

        isAddition = Random.value > 0.5f;

        if (!isAddition && b > a)
        {
            int temp = a;
            a = b;
            b = temp;
        }

        result = isAddition ? a + b : a - b;

        operatorIcon.text = isAddition ? "+" : "-";
        problemText.text = a + " " + operatorIcon.text + " " + b + " = ?";

        ShowObjects(groupA, a);
        ShowObjects(groupB, b);

        GenerateOptions();
    }

    void ShowObjects(Transform parent, int count)
    {
        Sprite sprite = objectSprites[Random.Range(0, objectSprites.Count)];

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(objectPrefab, parent);
            obj.GetComponent<Image>().sprite = sprite;
        }
    }

    void ClearObjects()
    {
        foreach (Transform t in groupA) Destroy(t.gameObject);
        foreach (Transform t in groupB) Destroy(t.gameObject);
    }

    void GenerateOptions()
    {
        int correctIndex = Random.Range(0, optionButtons.Length);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int value;

            if (i == correctIndex)
                value = result;
            else
                value = result + Random.Range(-3, 4);

            if (value < 0) value = Random.Range(0, 5);

            optionButtons[i].GetComponentInChildren<TMP_Text>().text = value.ToString();

            int captured = value;

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => CheckAnswer(captured));
        }
    }

    void CheckAnswer(int answer)
    {
        if (answer == result)
        {
            feedbackText.text = "¡Muy bien!";
            feedbackText.color = Color.green;
            btnNext.gameObject.SetActive(true);
        }
        else
        {
            feedbackText.text = "Inténtalo de nuevo";
            feedbackText.color = Color.red;
        }
    }
    public void Menu()
    {
        int edad = HistorialManager.ObtenerEdadGuardada();

        if (edad == 1)
        {
            SceneManager.LoadScene("03_Levels_2_4");
        }
        else
        {
            SceneManager.LoadScene("04_Levels_5_7");
        }
    }
    public void Next()
    {
        btnNext.gameObject.SetActive(false);
        GenerateProblem();
    }
    public void Next127()
    {
        btnNext.gameObject.SetActive(false);
        GenerateProblem();
    }

    
}