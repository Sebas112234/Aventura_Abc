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
    public GameObject Instruccion;
    public float tiempoMensaje = 1.5f;
    public Color colorCompletado = Color.green;

    public GameObject botonMenu;
    public GameObject botonMenuPanel;

    // --- VARIABLES NUEVAS PARA EL HISTORIAL ---
    private int totalAciertos = 0;
    private int totalErrores = 0;
    private const string NOMBRE_JUEGO = "Alfabeto Trazado"; // Cambia esto al nombre real de tu juego
    // ------------------------------------------

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

        for (int i = 0; i < letras.Length; i++)
        {
            if (letras[i] != null)
                letras[i].SetActive(i == index);
        }
    }

    public void CompletarLetra(int index)
    {
        if (index < 0 || index >= letrasCompletadas.Length) return;
        if (letrasCompletadas[index]) return;

        letrasCompletadas[index] = true;

        // --- LÓGICA DE HISTORIAL ---
        totalAciertos++;
        // Se envía: Nombre, aciertos, errores, rondasExitosas (1), rondasFallidas (0)
        HistorialManager.GuardarOActualizarProgreso(NOMBRE_JUEGO, totalAciertos, totalErrores, 1, 0);
        // ---------------------------

        if (index < botonesLetras.Length && botonesLetras[index] != null)
        {
            Button btn = botonesLetras[index];
            Transform completado = btn.transform.Find("CuadroCompletado");

            if (completado != null)
                completado.gameObject.SetActive(true);

            btn.interactable = false;
        }

        StartCoroutine(MostrarMensajeYRegresar());
    }

    // NUEVA FUNCIÓN: Llama a esto desde ControlTrazo cuando el usuario cometa un error
    public void RegistrarError()
    {
        totalErrores++;
        HistorialManager.GuardarOActualizarProgreso(NOMBRE_JUEGO, totalAciertos, totalErrores, 0, 1);
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