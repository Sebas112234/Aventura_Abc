using UnityEngine;
using UnityEngine.SceneManagement;

public class Levels_5_7_UI : MonoBehaviour
{
    public void LoadLevel(int levelNumber)
    {
        Debug.Log("Cargando Nivel 5-7: " + levelNumber);
    }

    public void Back()
    {
        SceneManager.LoadScene("02_AgeSelect");
    }
    public void SimonDice()
    {
        SceneManager.LoadScene("SimonDice");
    }
    public void ContarObjetos()
    {
        SceneManager.LoadScene("ContarObjetos");
    }
    public void TrazadoLetras()
    {
        SceneManager.LoadScene("TrazadoLetras");
    }
    public void TrazadoFiguras()
    {
        SceneManager.LoadScene("TrazarFiguras");
    }
    public void EscucharPalabras()
    {
        SceneManager.LoadScene("07_ListenWords");
    }
    public void FormarPalabras()
    {
        SceneManager.LoadScene("FormarPalabras");
    }
    public void SumasRestas()
    {
        SceneManager.LoadScene("06_Math_5_7");
    }
    public void EncontrarPalabras()
    {
        SceneManager.LoadScene("EncontrarPalabras");
    }
    public void Memorama()
    {
        SceneManager.LoadScene("Memorama");
    }

}
