using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryGame : MonoBehaviour
{
    [Header("Configuración de UI")]
    public GameObject cardPrefab;
    public Transform gridParent;

    private List<Color> colors = new List<Color> {
        Color.red, Color.red,
        Color.blue, Color.blue,
        Color.green, Color.green,
        Color.yellow, Color.yellow,
        Color.magenta, Color.magenta,
        Color.cyan, Color.cyan
    };

    private GameObject firstSelected;
    private GameObject secondSelected;
    private bool isChecking = false;

    void Start()
    {
        Shuffle(colors);
        SetupGrid();
    }

    void SetupGrid()
    {
        for (int i = 0; i < colors.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, gridParent);
            Image cardImage = newCard.transform.Find("ColorOverlay").GetComponent<Image>();
            cardImage.color = colors[i];
            
            // Ocultar el color al inicio
            cardImage.gameObject.SetActive(false);

            Button btn = newCard.GetComponent<Button>();
            int index = i; // Capturar índice para el listener
            btn.onClick.AddListener(() => OnCardClicked(newCard, cardImage.gameObject));
        }
    }

    void OnCardClicked(GameObject card, GameObject colorOverlay)
    {
        if (isChecking || colorOverlay.activeSelf || card == firstSelected) return;

        colorOverlay.SetActive(true);

        if (firstSelected == null)
        {
            firstSelected = card;
        }
        else
        {
            secondSelected = card;
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        isChecking = true;
        
        Color color1 = firstSelected.transform.Find("ColorOverlay").GetComponent<Image>().color;
        Color color2 = secondSelected.transform.Find("ColorOverlay").GetComponent<Image>().color;

        yield return new WaitForSeconds(0.8f);

        if (color1 != color2)
        {
            // Si no son iguales, se ocultan
            firstSelected.transform.Find("ColorOverlay").gameObject.SetActive(false);
            secondSelected.transform.Find("ColorOverlay").gameObject.SetActive(false);
        }

        firstSelected = null;
        secondSelected = null;
        isChecking = false;
    }

    void Shuffle(List<Color> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Color temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}