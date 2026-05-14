using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MemoryGame : MonoBehaviour
{
    [Header("Configuración de UI")]
    public GameObject cardPrefab;
    public Transform gridParent;
    public GameObject victoryText;

    [Header("Sprites de las Cartas")]
    public List<Sprite> cardSprites; 

    private List<GameObject> spawnedCards = new List<GameObject>();
    private GameObject firstSelected;
    private GameObject secondSelected;
    private bool isChecking = false;
    private int pairsFound = 0;
    private int totalPairs;

    void Start()
    {
        totalPairs = cardSprites.Count / 2;
        SetupGame();
    }

    void SetupGame()
    {
        //limpiamos datos anteriores
        pairsFound = 0;
        victoryText.SetActive(false);
        
        //borramos cartas viejas si existen
        foreach (GameObject card in spawnedCards) Destroy(card);
        spawnedCards.Clear();

        Shuffle(cardSprites);
        SetupGrid();
    }

    void SetupGrid()
    {
        for (int i = 0; i < cardSprites.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, gridParent);
            spawnedCards.Add(newCard); 

            Image cardImage = newCard.transform.Find("ColorOverlay").GetComponent<Image>();
            cardImage.sprite = cardSprites[i];
            cardImage.color = Color.white; 
            cardImage.gameObject.SetActive(false);

            TextMeshProUGUI buttonText = newCard.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = "?";

            Button btn = newCard.GetComponent<Button>();
            btn.onClick.AddListener(() => OnCardClicked(newCard, cardImage.gameObject));
        }
    }

    void OnCardClicked(GameObject card, GameObject imageOverlay)
    {
        if (isChecking || imageOverlay.activeSelf || card == firstSelected) 
            return;

        imageOverlay.SetActive(true);

        if (firstSelected == null)
            firstSelected = card;
        else{
            secondSelected = card;
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        isChecking = true;
        
        Sprite sprite1 = firstSelected.transform.Find("ColorOverlay").GetComponent<Image>().sprite;
        Sprite sprite2 = secondSelected.transform.Find("ColorOverlay").GetComponent<Image>().sprite;

        yield return new WaitForSeconds(0.6f);

        if (sprite1 == sprite2)
        {
            firstSelected.GetComponent<Button>().interactable = false;
            secondSelected.GetComponent<Button>().interactable = false;
            pairsFound++;
            
            if (pairsFound >= totalPairs)
            {
                victoryText.SetActive(true);
                StartCoroutine(RegresoAutomaticoMenu());
            }
        }
        else
        {
            firstSelected.transform.Find("ColorOverlay").gameObject.SetActive(false);
            secondSelected.transform.Find("ColorOverlay").gameObject.SetActive(false);
        }

        firstSelected = null;
        secondSelected = null;
        isChecking = false;
    }

    IEnumerator RegresoAutomaticoMenu()
    {
        yield return new WaitForSeconds(3.5f);
        Menu();
    }

    public void RestartGame()
    {
        if (isChecking) return;
        SetupGame();
    }

    void Shuffle(List<Sprite> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Sprite temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void Menu()
    {
        int edad = HistorialManager.ObtenerEdadGuardada();
        if (edad == 1) 
            SceneManager.LoadScene("03_Levels_2_4");
        else   
            SceneManager.LoadScene("04_Levels_5_7");
    }
}