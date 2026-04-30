using UnityEngine;

public class PruebaHistorial : MonoBehaviour
{
    // Esta función la llamaremos desde un botón
    public void GenerarDatosFicticios()
    {
        Debug.Log("Generando datos de prueba...");
        
        // Simulamos que jugamos 3 juegos diferentes
        HistorialManager.GuardarOActualizarProgreso("Trazado de Letras", 12, 2, 5, 1);
        HistorialManager.GuardarOActualizarProgreso("Contar Objetos", 8, 0, 3, 0);
        HistorialManager.GuardarOActualizarProgreso("Suma Animal", 20, 5, 10, 2);

        Debug.Log("¡Datos generados! Abre la tabla para verlos.");
    }
}