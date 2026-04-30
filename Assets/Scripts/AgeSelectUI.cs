using UnityEngine;
using UnityEngine.SceneManagement;

public class AgeSelectUI : MonoBehaviour
{
    public void Select3to5()
    {
        PlayerPrefs.SetInt("AgeGroup", 1);
        PlayerPrefs.Save(); // Forzamos el guardado en disco
        SceneManager.LoadScene("Levels_2_4"); // Cambié a la escena de niveles correspondiente
    }

    public void Select6to8()
    {
        PlayerPrefs.SetInt("AgeGroup", 2);
        PlayerPrefs.Save(); 
        SceneManager.LoadScene("04_Levels_5_7"); 
    }

    public void Back()
    {
        SceneManager.LoadScene("01_Welcome");
    }
}
