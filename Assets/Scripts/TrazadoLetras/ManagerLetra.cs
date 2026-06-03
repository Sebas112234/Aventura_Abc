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
    public GameObject botonReintentar;

    private int totalAciertos = 0;
    private int totalErrores = 0;
    private const string NOMBRE_JUEGO = "Trazado Letras"; 

    private bool[] letrasCompletadas = new bool[26];
    private int letraActual = -1;

    public AndroidTTS tts;

    void Start()
    {
        tts = FindObjectOfType<AndroidTTS>();

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
        botonReintentar.SetActive(true);

        letraActual = index;

        panelAlfabeto.SetActive(false);
        Instruccion.SetActive(false);
        contenedorLet.SetActive(true);

        for (int i = 0; i < letras.Length; i++)
        {
            if (letras[i] != null)
                letras[i].SetActive(i == index);
        }

        ReproducirInstruccionLetra(index);
    }

    public void CompletarLetra(int index)
    {
        if (index < 0 || index >= letrasCompletadas.Length) return;
        if (letrasCompletadas[index]) return;

        letrasCompletadas[index] = true;

        //historial
        totalAciertos++;
        HistorialManager.GuardarOActualizarProgreso(NOMBRE_JUEGO, totalAciertos, totalErrores, 1, 0);
      

        if (index < botonesLetras.Length && botonesLetras[index] != null)
        {
            Button btn = botonesLetras[index];
            Transform completado = btn.transform.Find("CuadroCompletado");

            if (completado != null)
                completado.gameObject.SetActive(true);

            btn.interactable = false;
        }

        Hablar("Letra completada, felicidades");
        StartCoroutine(MostrarMensajeYRegresar());
    }

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
        botonReintentar.SetActive(false);
        contenedorLet.SetActive(false);
        panelAlfabeto.SetActive(true);
        Instruccion.SetActive(true);
    }

    void ReproducirInstruccionLetra(int index)
    {
#if UNITY_EDITOR
        Debug.Log("Letra " + (char)('A' + index) + ", sigue la línea de trazo");
        return;
#endif

        if (tts == null)
        {
            Debug.LogWarning("TTS no asignado");
            return;
        }

        if (!tts.IsReady)
        {
            Debug.LogWarning("TTS aún no está listo");
            return;
        }

        string letra = ((char)('A' + index)).ToString();

        tts.Speak("Letra " + letra + ", sigue la línea de trazo");
    }

    public void Hablar(string texto)
    {
#if UNITY_EDITOR
        Debug.Log(texto);
        return;
#endif

        if (tts == null)
        {
            Debug.LogWarning("TTS no asignado");
            return;
        }

        if (!tts.IsReady)
        {
            Debug.LogWarning("TTS aún no está listo");
            return;
        }

        tts.Speak(texto);
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