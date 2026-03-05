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
        cantidad1 = Random.Range(1, 10);
        cantidad2 = Random.Range(1, 10);
        cantidad3 = Random.Range(1, 10);

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

            Instantiate(ObjetoPrefab, Espacio).GetComponent<RectTransform>().anchoredPosition = posicion;
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