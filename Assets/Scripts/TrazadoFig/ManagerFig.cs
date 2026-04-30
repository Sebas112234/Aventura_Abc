using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ManagerFig : MonoBehaviour
{
    public GameObject PanelFig;
    public GameObject ContenedorFig;
    public GameObject[] figuras;
    public Button[] botonesFiguras;
    public GameObject MensajeCompletado;
    //public TextMeshProUGUI textoCompletado;
    public GameObject Instruccion;
    public float tiempoMensaje = 1.5f;
    public Color colorCompletado = Color.green;

    private bool[] FigurasCompletadas = new bool[7];

    private int FiguraActual = -1;


    void Start()
    {
        if (MensajeCompletado != null)
            MensajeCompletado.gameObject.SetActive(false);

        if (ContenedorFig != null)
            ContenedorFig.SetActive(false);

        for (int i = 0; i < figuras.Length; i++)
        {
            if (figuras[i] != null)
                figuras[i].SetActive(false);
        }
    }

    public void MostrarFig(int index)
    {
        Debug.Log("CLICK FUNCIONA");
        FiguraActual = index;

        PanelFig.SetActive(false);
        Instruccion.SetActive(false);
        ContenedorFig.SetActive(true);

        Debug.Log("Mostrar letra index: " + index);
        Debug.Log("Letra: " + figuras[index].name);

        for (int i = 0; i < figuras.Length; i++)
        {
            if (figuras[i] != null)
                figuras[i].SetActive(i == index);
            Debug.Log("Activando: " + i);
        }
    }

    public void CompletarFig(int index)
    {
        if (index < 0 || index >= FigurasCompletadas.Length) return;
        if (FigurasCompletadas[index]) return;

        FigurasCompletadas[index] = true;

        if (index < botonesFiguras.Length && botonesFiguras[index] != null)
        {
            Button btn = botonesFiguras[index];

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
            ContenedorFig.SetActive(false);
            MensajeCompletado.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(tiempoMensaje);

        if (MensajeCompletado != null)
        {
            MensajeCompletado.gameObject.SetActive(false);
        }

        ContenedorFig.SetActive(false);
        PanelFig.SetActive(true);
        Instruccion.SetActive(true);
    }

    public void ReintentarFig()
    {
        if (FiguraActual < 0) return;

        TrazoFig tf = figuras[FiguraActual].GetComponent<TrazoFig>();

        if (tf != null)
            tf.ReintentarTrazo();
    }

    public void RegresarPanel()
    {
        ContenedorFig.SetActive(false);
        PanelFig.SetActive(true);
        Instruccion.SetActive(true);

    }
}