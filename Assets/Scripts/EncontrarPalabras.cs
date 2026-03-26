using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ControladorJuego : MonoBehaviour
{
    [Header("Referencias de UI")]
    public TextMeshProUGUI textoDisplay;
    public Button[] botonesLetras;
    public Button botonValidar;
    public Button botonReintentar; 

    [Header("Configuración de Letras")]
    public string abecedarioPosible = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ"; 
    
    private string[] letrasGeneradas = new string[7]; 
    private string palabraFormada = "";

    void Start()
    {
        if (botonValidar != null)
            botonValidar.onClick.AddListener(Validar);

        if (botonReintentar != null)
            botonReintentar.onClick.AddListener(ReiniciarMazo);

        ReiniciarMazo();
    }

    public void ReiniciarMazo()
    {
        palabraFormada = "";
        textoDisplay.text = "";

        GenerarLetras();

        ActualizarTextoBotones();
    }

    void GenerarLetras()
    {
        List<char> bolsaDeLetras = new List<char>(abecedarioPosible.ToCharArray());

        if (bolsaDeLetras.Count < 7)
        {
            Debug.LogError("El abecedario debe tener al menos 7 letras únicas.");
            return;
        }

        for (int i = 0; i < 7; i++)
        {
            int indiceAleatorio = Random.Range(0, bolsaDeLetras.Count);
            letrasGeneradas[i] = bolsaDeLetras[indiceAleatorio].ToString();
            bolsaDeLetras.RemoveAt(indiceAleatorio);
        }
    }

    void ActualizarTextoBotones()
    {
        for (int i = 0; i < botonesLetras.Length; i++)
        {
            int indice = i; 
            TextMeshProUGUI textoDelBoton = botonesLetras[i].GetComponentInChildren<TextMeshProUGUI>();
            
            if (textoDelBoton != null)
                textoDelBoton.text = letrasGeneradas[i];

            botonesLetras[i].onClick.RemoveAllListeners(); 
            botonesLetras[i].onClick.AddListener(() => AñadirLetra(letrasGeneradas[indice]));
        }
    }

    void AñadirLetra(string letra)
    {
        palabraFormada += letra;
        textoDisplay.text = palabraFormada;
    }

    void Validar()
    {
        Debug.Log("Validación fallida. Borrando palabra...");
        palabraFormada = ""; 
        textoDisplay.text = palabraFormada;
    }
}