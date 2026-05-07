using UnityEngine;
using TMPro;

public class SyllableSlot : MonoBehaviour {
    public TextMeshProUGUI textMesh;
    public bool isPlaceholder; // Se marca como TRUE solo para el hueco "-"

    public void Setup(string txt, bool placeholder) {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        textMesh.text = txt;
        isPlaceholder = placeholder;
        
        // Si es el hueco, le ponemos un nombre especial para encontrarlo por código
        if (isPlaceholder) gameObject.name = "HUECO_VALIDO";
        else gameObject.name = "Silaba_Fija";
    }
}