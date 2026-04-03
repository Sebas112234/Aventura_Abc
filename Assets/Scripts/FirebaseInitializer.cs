using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseInitializer : MonoBehaviour
{
    public static bool IsReady { get; private set; }

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                IsReady = true;
                Debug.Log("Firebase listo");
            }
            else
            {
                Debug.LogError("Firebase no disponible: " + task.Result);
            }
        });
    }
}