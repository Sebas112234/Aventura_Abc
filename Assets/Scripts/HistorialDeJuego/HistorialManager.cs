using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

// ESTA PARTE DEFINE LOS DATOS
[System.Serializable]
public class RegistroJuego {
    public string nombreMinijuego;
    public int aciertos;
    public int errores;
    public int rondasExitosas;
    public int rondasFallidas;
    public float porcentajeExito;

    public void CalcularPorcentaje() {
        int total = rondasExitosas + rondasFallidas;
        porcentajeExito = total > 0 ? (float)rondasExitosas / total * 100f : 0f;
    }
}

[System.Serializable]
public class HistorialGeneral {
    public List<RegistroJuego> listaRegistros = new List<RegistroJuego>();
}

// ESTA PARTE ES EL MANAGER
public class HistorialManager : MonoBehaviour {
    private static string PathPersistente => Path.Combine(Application.persistentDataPath, "historial.json");

    public static void GuardarOActualizarProgreso(string nombre, int aciertos, int errores, int exitoRonda, int fallaRonda) {
        HistorialGeneral data = CargarHistorial();
        RegistroJuego registro = data.listaRegistros.FirstOrDefault(r => r.nombreMinijuego == nombre);

        if (registro == null) {
            registro = new RegistroJuego { nombreMinijuego = nombre };
            data.listaRegistros.Add(registro);
        }

        registro.aciertos = aciertos;
        registro.errores = errores;
        registro.rondasExitosas = exitoRonda;
        registro.rondasFallidas = fallaRonda;
        registro.CalcularPorcentaje();

        File.WriteAllText(PathPersistente, JsonUtility.ToJson(data, true));
    }

    public static HistorialGeneral CargarHistorial() {
        if (!File.Exists(PathPersistente)) return new HistorialGeneral();
        return JsonUtility.FromJson<HistorialGeneral>(File.ReadAllText(PathPersistente));
    }
}