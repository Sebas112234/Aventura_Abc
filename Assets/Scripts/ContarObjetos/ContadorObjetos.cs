using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ContadorObjetos : MonoBehaviour
{
    public GameObject ObjetoPrefab;
    public ManagerContarObj manager;

    public Transform Espacio1;
    public Transform Espacio2;
    public Transform Espacio3;
    public GameObject MensajeNivel;
    public GameObject MensajeJuego;
    public GameObject MensajeReintentar;


    int cantidad1;
    int cantidad2;
    int cantidad3;

    int respuestaCorrecta;

    void Start()
    {
        manager = FindObjectOfType<ManagerContarObj>();
    }

    void GenerarNivel()
    {
        // Generar tres cantidades distintas entre 1 y 9
        int min = 1;
        int max = 10; // Random.Range int es inclusive min, exclusive max

        cantidad1 = Random.Range(min, max);
        // asegurar que cantidad2 sea diferente a cantidad1
        do { cantidad2 = Random.Range(min, max); } while (cantidad2 == cantidad1);
        // asegurar que cantidad3 sea diferente a cantidad1 y cantidad2
        do { cantidad3 = Random.Range(min, max); } while (cantidad3 == cantidad1 || cantidad3 == cantidad2);

        GenerarManzanas(Espacio1, cantidad1);
        GenerarManzanas(Espacio2, cantidad2);
        GenerarManzanas(Espacio3, cantidad3);

        int mayor = Mathf.Max(cantidad1, cantidad2, cantidad3);

        if (cantidad1 == mayor) respuestaCorrecta = 1;
        else if (cantidad2 == mayor) respuestaCorrecta = 2;
        else respuestaCorrecta = 3;
    }

    public void GenerarNuevoNivel()
    {
        LimpiarEspacios();
        GenerarNivel();
    }

    void GenerarManzanas(Transform Espacio, int cantidad)
    {
        float distanciaMinima = 60f;

        List<Vector2> posicionesUsadas = new List<Vector2>();

        for (int i = 0; i < cantidad; i++)
        {
            Vector2 posicion;
            bool posicionValida;

            int intentos = 0;

            do
            {
                posicion = new Vector2(
                    Random.Range(-100, 100),
                    Random.Range(-100, 100)
                );

                posicionValida = true;

                foreach (Vector2 pos in posicionesUsadas)
                {
                    if (Vector2.Distance(posicion, pos) < distanciaMinima)
                    {
                        posicionValida = false;
                        break;
                    }
                }

                intentos++;

            } while (!posicionValida && intentos < 50);

            posicionesUsadas.Add(posicion);

            GameObject go = Instantiate(ObjetoPrefab, Espacio, false);
            go.transform.localPosition = posicion;
            go.name = "Manzana_" + i;
        }
    }

    void LimpiarEspacios()
    {
        foreach (Transform hijo in Espacio1)
            Destroy(hijo.gameObject);

        foreach (Transform hijo in Espacio2)
            Destroy(hijo.gameObject);

        foreach (Transform hijo in Espacio3)
            Destroy(hijo.gameObject);
    }

    public void Seleccionar(int opcion)
    {
        if (opcion == respuestaCorrecta)
        {
            if (manager.nivelActual < 10)
            {
                MensajeNivel.SetActive(true);
                manager.StartCoroutine(manager.OcultarMensaje(MensajeNivel));
            }

            manager.NivelCompletado();
        }
        else
        {
            MensajeReintentar.SetActive(true);
            manager.StartCoroutine(manager.OcultarMensaje(MensajeReintentar));
        }
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