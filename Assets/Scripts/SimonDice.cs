using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SimonDice : MonoBehaviour
{
    [Header("UI Elements")]
    public Image panelVisualizador; 
    public Button[] botonesColores; 
    public TextMeshProUGUI textoMensaje; 
    public TextMeshProUGUI textoNivel;   

    [Header("Configuración")]
    public Color[] coloresSecuencia; 
    public float tiempoMuestra = 0.7f;
    public float tiempoEspera = 0.3f;
    
    private List<int> secuenciaMaestra = new List<int>();
    private int limiteActual = 1; 
    private int indiceUsuario = 0; 
    private bool puedeJugar = false;

    void Start()
    {
        if (textoMensaje == null || textoNivel == null) return;
        PrepararJuego();
    }

    void PrepararJuego()
    {
        secuenciaMaestra.Clear();
        for (int i = 0; i < 5; i++)
        {
            secuenciaMaestra.Add(Random.Range(0, botonesColores.Length));
        }
        limiteActual = 1;
        ActualizarUI();
        StartCoroutine(ReproducirSecuencia());
    }

    void ActualizarUI()
    {
        if(textoNivel != null)
            textoNivel.text = "Nivel " + limiteActual + "/5";
    }

    IEnumerator ReproducirSecuencia()
    {
        puedeJugar = false;
        indiceUsuario = 0;
        
        textoMensaje.color = Color.black; 
        textoMensaje.text = "Memoriza la secuencia...";
        
        panelVisualizador.color = Color.white; 
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < limiteActual; i++)
        {
            panelVisualizador.color = coloresSecuencia[secuenciaMaestra[i]];
            yield return new WaitForSeconds(tiempoMuestra);
            panelVisualizador.color = Color.white;
            yield return new WaitForSeconds(tiempoEspera);
        }

        textoMensaje.color = new Color(0.2f, 0.2f, 0.2f, 1f); //gris oscuro
        textoMensaje.text = "¡Tu turno!";
        panelVisualizador.color = new Color(1, 1, 1, 0.5f); 
        puedeJugar = true;
    }

    public void PresionBoton(int idBoton)
    {
        if (!puedeJugar) return;

        if (idBoton == secuenciaMaestra[indiceUsuario])
        {
            indiceUsuario++;
            if (indiceUsuario >= limiteActual)
            {
                if (limiteActual < 5)
                {
                    limiteActual++;
                    ActualizarUI();
                    textoMensaje.color = Color.green; // VERDE
                    textoMensaje.text = "¡Excelente!";
                    StartCoroutine(ReproducirSecuencia());
                }
                else
                {
                    textoMensaje.color = Color.green;
                    textoMensaje.text = "¡Juego Terminado! \n Lo lograste";
                    StartCoroutine(RegresoAutomaticoMenu());
                }
            }
        }
        else
        {
            StartCoroutine(FeedbackError());
        }
    }

    IEnumerator FeedbackError()
    {
        puedeJugar = false;
        textoMensaje.color = Color.red; //rojo
        textoMensaje.text = "Intenta de nuevo \n Repitiendo...";
        panelVisualizador.color = new Color(1, 0, 0, 0.3f); 
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(ReproducirSecuencia());
    }

    IEnumerator RegresoAutomaticoMenu()
    {
        puedeJugar = false;
        yield return new WaitForSeconds(3.5f);
        Menu();
    }

    public void BotonReintentar()
    {
        StopAllCoroutines();
        limiteActual = 1; 
        ActualizarUI();
        StartCoroutine(ReproducirSecuencia());
    }

    public void Menu()
    {
        int edad = HistorialManager.ObtenerEdadGuardada();
        if (edad == 1) 
            SceneManager.LoadScene("Levels_2_4");
        else   
            SceneManager.LoadScene("04_Levels_5_7");
    }
}