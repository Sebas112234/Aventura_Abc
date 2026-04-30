using UnityEngine;
using TMPro; // <--- Si esta línea no está, no funcionará

public class FilaUI : MonoBehaviour
{
    // Asegúrate de que diga TextMeshProUGUI
    public TextMeshProUGUI TxtNombre;
    public TextMeshProUGUI TxtAciertos;
    public TextMeshProUGUI TxtErrores;
    public TextMeshProUGUI TxtPorcentaje;

    public void Configurar(RegistroJuego datos)
    {
        TxtNombre.text = datos.nombreMinijuego;
        TxtAciertos.text = "A: " + datos.aciertos;
        TxtErrores.text = "E: " + datos.errores;
        TxtPorcentaje.text = datos.porcentajeExito.ToString("F1") + "%";
    }
}