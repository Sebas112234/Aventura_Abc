using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ManagerContarObj : MonoBehaviour
{
    public ContadorObjetos contador;

    public int nivelActual = 1;
    int totalNiveles = 10;

    void Start()
    {
        Debug.Log("Nivel " + nivelActual);
        contador.GenerarNuevoNivel();
    }

    public void NivelCompletado()
    {
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

    public IEnumerator OcultarMensaje(GameObject x)
    {
        yield return new WaitForSeconds(1.2f);
        x.SetActive(false);
    }
}