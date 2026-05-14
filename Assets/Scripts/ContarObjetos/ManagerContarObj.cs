using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ManagerContarObj : MonoBehaviour
{
    public ContadorObjetos contador;

    public int nivelActual = 1;
    int totalNiveles = 10;

    // --- VARIABLES PARA EL HISTORIAL ---
    private int totalAciertos = 0;
    private int totalErrores = 0;
    private int rondasExitosas = 0;
    private int rondasFallidas = 0;
    private const string NOMBRE_JUEGO = "Contar Objetos";
    // -----------------------------------

    void Start()
    {
        Debug.Log("Nivel " + nivelActual);
        contador.GenerarNuevoNivel();
    }

    public void NivelCompletado()
    {
        // --- ACTUALIZAR HISTORIAL POR NIVEL LOGRADO ---
        totalAciertos++;
        rondasExitosas++;
        ActualizarHistorial();
        // ----------------------------------------------

        nivelActual++;

        if (nivelActual <= totalNiveles)
        {
            Debug.Log("Nivel " + nivelActual);
            contador.GenerarNuevoNivel();
        }
        else
        {
            contador.MensajeJuego.SetActive(true);
            StartCoroutine(OcultarMensaje(contador.MensajeJuego));
            SceneManager.LoadScene("03_Levels_2_4");
        }
    }

    // Función para que el contador reporte cuando el niño se equivoca
    public void RegistrarFallo()
    {
        totalErrores++;
        rondasFallidas++;
        ActualizarHistorial();
    }

    private void ActualizarHistorial()
    {
        HistorialManager.GuardarOActualizarProgreso(NOMBRE_JUEGO, totalAciertos, totalErrores, rondasExitosas, rondasFallidas);
    }

    public IEnumerator OcultarMensaje(GameObject x)
    {
        yield return new WaitForSeconds(1.2f);
        x.SetActive(false);
    }
}