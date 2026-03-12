using UnityEngine;
using UnityEngine.SceneManagement;

public class WelcomeUI : MonoBehaviour
{
    public void OnStartPressed()
    {
        SceneManager.LoadScene("02_AgeSelect");
    }
}
