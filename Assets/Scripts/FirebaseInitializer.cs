using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseInitializer : MonoBehaviour
{
    public static bool IsReady { get; private set; } = false;

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;
            Debug.Log("Firebase dependency status: " + status);

            if (status == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                IsReady = true;
                Debug.Log("Firebase listo");
            }
            else
            {
                IsReady = false;
                Debug.LogError("Firebase no disponible: " + status);
            }
        });
    }
}