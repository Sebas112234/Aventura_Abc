using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimonDice : MonoBehaviour
{
    [Header("UI Elements")]
    public Image panelVisualizador; //el cuadro grande central
    public Button[] botonesColores; //los 4 botones de abajo

    [Header("Configuración")]
    public Color[] coloresSecuencia; //0 = Rojo, 1 = Amarillo, 2 = Verde, 3 = Azul
    
    private List<int> secuenciaMaestra = new List<int>();
    private int limiteActual = 1; //cuántos pasos de la secuencia mostramos
    private int indiceUsuario = 0; //por qué color va el niño presionando
    private bool puedeJugar = false;

    void Start()
    {
        PrepararJuego();
    }

    void PrepararJuego()
    {
        secuenciaMaestra.Clear();
        //se genera la secuencia completa de 5 pasos desde el inicio
        for (int i = 0; i < 5; i++)
        {
            secuenciaMaestra.Add(Random.Range(0, 4));
        }
        limiteActual = 1;
        StartCoroutine(ReproducirSecuencia());
    }

    IEnumerator ReproducirSecuencia()
    {
        puedeJugar = false;
        indiceUsuario = 0;
        panelVisualizador.color = Color.white; //estado neutro
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < limiteActual; i++)
        {
            //mostrar el color que corresponde en la lista
            panelVisualizador.color = coloresSecuencia[secuenciaMaestra[i]];
            yield return new WaitForSeconds(0.7f);
            panelVisualizador.color = Color.white;
            yield return new WaitForSeconds(0.3f);
        }

        panelVisualizador.color = new Color(1, 1, 1, 0.5f); //color gris/tenue: "Tu turno"
        puedeJugar = true;
    }

    public void PresionBoton(int idBoton)
    {
        if (!puedeJugar) return;

        //comprobar si el botón que tocó es el que sigue en la lista
        if (idBoton == secuenciaMaestra[indiceUsuario])
        {
            indiceUsuario++;
            
            //si terminó la secuencia actual
            if (indiceUsuario >= limiteActual)
            {
                if (limiteActual < 5)
                {
                    limiteActual++;
                    StartCoroutine(ReproducirSecuencia());
                }
                else
                {
                    Debug.Log("¡Ganaste el nivel!");
                }
            }
        }
        else
        {
            //error: el niño debe repetir la secuencia que llevaba
            StartCoroutine(FeedbackError());
        }
    }

    IEnumerator FeedbackError()
    {
        puedeJugar = false;
        panelVisualizador.color = Color.black; //o un texto que diga "Reintentar"
        Debug.Log("Error: Repitiendo secuencia...");
        yield return new WaitForSeconds(1f);
        StartCoroutine(ReproducirSecuencia());
    }

    public void BotonReintentar()
    {
        StopAllCoroutines();
        limiteActual = 1; //reinicia el progreso pero mantiene la misma secuenciaMaestra
        StartCoroutine(ReproducirSecuencia());
    }
}