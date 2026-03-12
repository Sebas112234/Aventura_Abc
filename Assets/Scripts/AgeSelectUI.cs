using UnityEngine;
using UnityEngine.SceneManagement;

public class AgeSelectUI : MonoBehaviour
{
    public void Select3to5()
    {
        PlayerPrefs.SetInt("AgeGroup", 1);
        SceneManager.LoadScene("01_Welcome"); // por ahora regresamos, luego será el menú
    }

    public void Select6to8()
    {
        PlayerPrefs.SetInt("AgeGroup", 2);
        SceneManager.LoadScene("04_Levels_5_7"); // por ahora regresamos, luego será el menú
    }

    public void Back()
    {
        SceneManager.LoadScene("01_Welcome");
    }
}
