using UnityEngine;
using UnityEngine.SceneManagement;

public class AgeSelectUI : MonoBehaviour
{
    public void Select3to5()
    {
        PlayerPrefs.SetInt("AgeGroup", 1);
        SceneManager.LoadScene("01_Welcome"); 
    }

    public void Select6to8()
    {
        PlayerPrefs.SetInt("AgeGroup", 2);
        SceneManager.LoadScene("04_Levels_5_7"); 
    }

    public void Back()
    {
        SceneManager.LoadScene("01_Welcome");
    }
}
