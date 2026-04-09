using UnityEngine;

public class TTSInitListener : AndroidJavaProxy
{
    private AndroidTTS ttsManager;

    public TTSInitListener(AndroidTTS manager)
        : base("android.speech.tts.TextToSpeech$OnInitListener")
    {
        ttsManager = manager;
    }

    public void onInit(int status)
    {
        Debug.Log("TTS onInit status: " + status);

        bool ok = (status == 0);
        ttsManager.SetReady(ok);

        if (ok)
            Debug.Log("TTS listo");
        else
            Debug.LogError("TTS falló al inicializar. status = " + status);
    }
}