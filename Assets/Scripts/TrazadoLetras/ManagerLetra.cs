using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class ManagerLetra : MonoBehaviour
{
    public GameObject panelAlfabeto;
    public GameObject contenedorLet;
    public GameObject[] letras;
    public Button[] botonesLetras;
    public GameObject MensajeCompletado;
    //public TextMeshProUGUI textoCompletado;
    public GameObject Instruccion;
    public float tiempoMensaje = 1.5f;
    public Color colorCompletado = Color.green;

    public GameObject botonMenu;
    public GameObject botonMenuPanel;


    private bool[] letrasCompletadas = new bool[26];

    private int letraActual = -1;


    void Start()
    {
        if (MensajeCompletado != null)
            MensajeCompletado.gameObject.SetActive(false);

        if (contenedorLet != null)
            contenedorLet.SetActive(false);

        for (int i = 0; i < letras.Length; i++)
        {
            if (letras[i] != null)
                letras[i].SetActive(false);
        }
    }

    public void MostrarLetra(int index)
    {
        botonMenu.SetActive(false);
        botonMenuPanel.SetActive(true);
        letraActual = index;

        panelAlfabeto.SetActive(false);
        Instruccion.SetActive(false);
        contenedorLet.SetActive(true);

        Debug.Log("Mostrar letra index: " + index);
        Debug.Log("Letra: " + letras[index].name);

        for (int i = 0; i < letras.Length; i++)
        {
            if (letras[i] != null)
                letras[i].SetActive(i == index);
            Debug.Log("Activando: " + i);
        }
    }

    public void CompletarLetra(int index)
    {
        if (index < 0 || index >= letrasCompletadas.Length) return;
        if (letrasCompletadas[index]) return;

        letrasCompletadas[index] = true;

        if (index < botonesLetras.Length && botonesLetras[index] != null)
        {
            Button btn = botonesLetras[index];

            Transform completado = btn.transform.Find("CuadroCompletado");

            if (completado != null)
                completado.gameObject.SetActive(true);

            btn.interactable = false;
        }

        Debug.Log("MOSTRANDO TEXTO");

        StartCoroutine(MostrarMensajeYRegresar());
    }

    private IEnumerator MostrarMensajeYRegresar()
    {
        if (MensajeCompletado != null)
        {
            contenedorLet.SetActive(false);
            MensajeCompletado.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(tiempoMensaje);

        if (MensajeCompletado != null)
        {
            MensajeCompletado.gameObject.SetActive(false);
        }

        contenedorLet.SetActive(false);
        panelAlfabeto.SetActive(true);
        Instruccion.SetActive(true);
    }

    public void ReintentarLetra()
    {
        if (letraActual < 0) return;

        ControlTrazo ct = letras[letraActual].GetComponent<ControlTrazo>();

        if (ct != null)
            ct.ReintentarTrazo();
    }

    public void RegresarPanel()
    {
        botonMenu.SetActive(true);
        botonMenuPanel.SetActive(false);
        contenedorLet.SetActive(false);
        panelAlfabeto.SetActive(true);
        Instruccion.SetActive(true);

    }

    public void Menu()
    {
        Debug.Log("Click Menu ");
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
}