using UnityEngine;

public class ContadorObjetos : MonoBehaviour
{
    public GameObject ObjetoPrefab;

    public Transform Espacio1;
    public Transform Espacio2;
    public Transform Espacio3;


    int cantidad1;
    int cantidad2;
    int cantidad3;

    int respuestaCorrecta;

    void Start()
    {
        GenerarNivel();
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

    void GenerarManzanas(Transform Espacio, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            Vector2 posicion = new Vector2(
                Random.Range(-100, 100),
                Random.Range(-100, 100)
            );

            // Instanciar como hijo sin mantener la misma posición y ajustar transformaciones
            GameObject go = Instantiate(ObjetoPrefab, Espacio, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = posicion;
                rt.localScale = ObjetoPrefab.transform.localScale;
            }
            else
            {
                go.transform.localPosition = new Vector3(posicion.x, posicion.y, 0f);
                go.transform.localScale = ObjetoPrefab.transform.localScale;
            }

            // Dar nombre único para facilitar la depuración
            go.name = ObjetoPrefab.name + "_" + i;
        }
    }

    public void Seleccionar(int opcion)
    {
        if (opcion == respuestaCorrecta)
        {
            Debug.Log("Correcto");
        }
        else
        {
            Debug.Log("Incorrecto");
        }
    }
}