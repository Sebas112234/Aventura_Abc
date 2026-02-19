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
}
