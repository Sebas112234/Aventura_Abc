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

    private int aciertos = 0;
    private int errores = 0;
    private string nombreJuego = "Simón Dice";

    private int maxNivelesRonda = 5; 

    void Start()
    {
        if (textoMensaje == null || textoNivel == null) return;
        PrepararJuego();
    }

    void PrepararJuego()
    {
        secuenciaMaestra.Clear();
        aciertos = 0;
        errores = 0;

        //CP3: Ajuste adaptativo de dificultad según la edad recuperada del HistorialManager
        int edadUsuario = HistorialManager.ObtenerEdadGuardada();
        if (edadUsuario >= 5 && edadUsuario <= 7)
        {
            maxNivelesRonda = 7; //aumenta la longitud para niños de 5 a 7 años
        }
        else
        {
            maxNivelesRonda = 5; //configuración estándar de 5 elementos para niños de 2 a 4 años
        }

        //CP2: Bucle corregido para evitar la generación de colores consecutivos idénticos
        int ultimoIndice = -1;
        for (int i = 0; i < maxNivelesRonda; i++)
        {
            int nuevoIndice;
            do
            {
                nuevoIndice = Random.Range(0, botonesColores.Length);
            } while (nuevoIndice == ultimoIndice); //forzar cambio si es igual al anterior

            secuenciaMaestra.Add(nuevoIndice);
            ultimoIndice = nuevoIndice; //actualizar el registro del último elemento añadido
        }

        limiteActual = 1;
        ActualizarUI();
        StartCoroutine(ReproducirSecuencia());
    }

    void ActualizarUI()
    {
        if(textoNivel != null)
            textoNivel.text = "Nivel " + limiteActual + "/" + maxNivelesRonda;
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

        textoMensaje.color = new Color(0.2f, 0.2f, 0.2f, 1f); 
        textoMensaje.text = "¡Tu turno!";
        panelVisualizador.color = new Color(1, 1, 1, 0.5f); 
        puedeJugar = true;
    }

    public void PresionBoton(int idBoton)
    {
        if (!puedeJugar) return;

        if (idBoton == secuenciaMaestra[indiceUsuario])
        {
            aciertos++; 
            indiceUsuario++;
            if (indiceUsuario >= limiteActual)
            {
                //CP3: Cambiado el límite "hardcoded" de 5 por la variable dinámica maxNivelesRonda
                if (limiteActual < maxNivelesRonda)
                {
                    limiteActual++;
                    ActualizarUI();
                    textoMensaje.color = Color.green; 
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
            errores++; 
            StartCoroutine(FeedbackError());
        }
    }

    IEnumerator FeedbackError()
    {
        puedeJugar = false;
        textoMensaje.color = Color.red; 
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
        aciertos = 0;
        errores = 0;
        limiteActual = 1; 
        ActualizarUI();
        StartCoroutine(ReproducirSecuencia());
    }

    public void Menu()
    {
        int rExito = (aciertos > errores) ? 1 : 0;
        int rFalla = (aciertos > errores) ? 0 : 1;

        if (aciertos > 0 || errores > 0)
        {
            HistorialManager.GuardarOActualizarProgreso(nombreJuego, aciertos, errores, rExito, rFalla);
        }

        int edad = HistorialManager.ObtenerEdadGuardada();
        if (edad == 1) 
            SceneManager.LoadScene("03_Levels_2_4");
        else   
            SceneManager.LoadScene("04_Levels_5_7");
    }
}