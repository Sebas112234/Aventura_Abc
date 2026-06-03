using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ManagerContarObj : MonoBehaviour
{
    public ContadorObjetos contador;

    public int nivelActual = 1;
    int totalNiveles = 10;

    private int totalAciertos = 0;
    private int totalErrores = 0;
    private int rondasExitosas = 0;
    private int rondasFallidas = 0;
    private const string NOMBRE_JUEGO = "Contar Objetos";


    void Start()
    {
        Debug.Log("Nivel " + nivelActual);
        contador.GenerarNuevoNivel();
    }

    public void NivelCompletado()
    {
        totalAciertos++;
        rondasExitosas++;
        ActualizarHistorial();

        nivelActual++;

        if (nivelActual <= totalNiveles)
        {
            Debug.Log("Nivel " + nivelActual);
            contador.GenerarNuevoNivel();
        }
        else
        {
            StartCoroutine(FinDelJuego());
        }
    }

    IEnumerator FinDelJuego()
    {
        contador.MensajeJuego.SetActive(true);

        yield return new WaitForSeconds(2.5f);

        SceneManager.LoadScene("03_Levels_2_4");
    }

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