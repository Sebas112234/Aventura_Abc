using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
        }
    }

    public IEnumerator OcultarMensaje(GameObject x)
    {
        yield return new WaitForSeconds(1.2f);
        x.SetActive(false);
    }
}