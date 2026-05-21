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
    private int aciertos = 0;
    private int errores = 0;
    private string nombreJuego = "Memorama";

    //CP1: Lista de persistencia temporal para recordar el orden de la ronda anterior
    private List<Sprite> secuenciaAnterior = new List<Sprite>();

    void Start()
    {
        //CP4: Cláusula de seguridad matemática para prevenir errores de asignación impar en Unity
        if (cardSprites != null && cardSprites.Count % 2 != 0)
        {
            Debug.LogWarning("Se detectó un número impar de Sprites en Memorama. Removiendo el elemento huérfano.");
            cardSprites.RemoveAt(cardSprites.Count - 1); // Trunca el elemento sobrante para mantener pares perfectos
        }

        totalPairs = cardSprites.Count / 2;
        SetupGame();
    }

    void SetupGame()
    {
        pairsFound = 0;
        aciertos = 0;
        errores = 0;
        victoryText.SetActive(false);
        
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
            aciertos++; 
            
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
            errores++; 
            
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

    //CP1: Algoritmo mejorado con guardián de coincidencia histórica secuencial externa
    void Shuffle(List<Sprite> list)
    {
        if (list.Count <= 1) return;

        bool esIdenticaALaAnterior = false;
        int intentosDeSeguridad = 0;

        do
        {
            for (int i = 0; i < list.Count; i++)
            {
                Sprite temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }

            //si hay un historial guardado, verificamos que la nueva lista no sea idéntica
            if (secuenciaAnterior.Count == list.Count)
            {
                esIdenticaALaAnterior = true;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != secuenciaAnterior[i])
                    {
                        esIdenticaALaAnterior = false; //se rompe la igualdad total, es un mazo válido
                        break;
                    }
                }
            }
            
            intentosDeSeguridad++;
            
        } while (esIdenticaALaAnterior && intentosDeSeguridad < 10); //reejecuta si es idéntica (máximo 10 intentos preventivos)

        //actualizamos la secuencia anterior reflejando el orden actual para la próxima partida
        secuenciaAnterior = new List<Sprite>(list);
    }

    public void Menu()
    {
        if (aciertos > 0 || errores > 0)
        {
            int rExito = (aciertos > errores) ? 1 : 0;
            int rFalla = (aciertos > errores) ? 0 : 1;

            HistorialManager.GuardarOActualizarProgreso(nombreJuego, aciertos, errores, rExito, rFalla);
        }

        int edad = HistorialManager.ObtenerEdadGuardada();
        if (edad == 1) 
            SceneManager.LoadScene("03_Levels_2_4");
        else   
            SceneManager.LoadScene("04_Levels_5_7");
    }
}