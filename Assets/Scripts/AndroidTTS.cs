using UnityEngine;

public class AndroidTTS : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject tts;
    private AndroidJavaObject activity;
    private bool isReady = false;
#endif

    public bool IsReady
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return isReady;
#else
            return false;
#endif
        }
    }

    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            TTSInitListener listener = new TTSInitListener(this);

            tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, listener);

            Debug.Log("Intentando inicializar TTS...");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error inicializando TTS: " + e.Message);
        }
#endif
    }

    public void Speak(string text)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!isReady || tts == null || string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("TTS no listo o texto vacío");
            return;
        }

        try
        {
            // Español genérico primero
            AndroidJavaObject locale = new AndroidJavaObject("java.util.Locale", "es", "ES");
            int result = tts.Call<int>("setLanguage", locale);
            Debug.Log("Resultado setLanguage: " + result);

            tts.Call<int>("speak", text, 0, null, "unity_tts");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al hablar: " + e.Message);
        }
#else
        Debug.Log("TTS solo funciona en Android real. Texto: " + text);
#endif
    }

    public void SetReady(bool ready)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        isReady = ready;
#endif
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (tts != null)
        {
            tts.Call("stop");
            tts.Call("shutdown");
            tts.Dispose();
            tts = null;
        }
#endif
    }
}