using UnityEngine;
using System.Collections.Generic;

public class VisualizadorHistorial : MonoBehaviour
{
    [Header("Configuración de Tabla")]
    public GameObject filaPrefab; // Aquí arrastra tu Prefab de la fila
    public Transform contentTransform; // Aquí arrastra el objeto 'Content' del ScrollView

    // Se ejecuta al abrir la pantalla o iniciar la escena
    void OnEnable()
    {
        DibujarTabla();
    }

    public void DibujarTabla()
    {
        // 1. Limpiar filas viejas para que no se dupliquen visualmente
        foreach (Transform hijo in contentTransform)
        {
            Destroy(hijo.gameObject);
        }

        // 2. Cargar los datos del archivo JSON
        HistorialGeneral datosGuardados = HistorialManager.CargarHistorial();

        // 3. Crear una fila por cada juego guardado
        foreach (RegistroJuego registro in datosGuardados.listaRegistros)
        {
            GameObject nuevaFila = Instantiate(filaPrefab, contentTransform);
            
            // Accedemos al script FilaUI para pasarle los datos
            FilaUI scriptFila = nuevaFila.GetComponent<FilaUI>();
            if (scriptFila != null)
            {
                scriptFila.Configurar(registro);
            }
        }
    }
}