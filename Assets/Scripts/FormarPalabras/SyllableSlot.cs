using UnityEngine;
using TMPro;

public class SyllableSlot : MonoBehaviour {
    public TextMeshProUGUI textMesh; // Arrastra el texto del botón aquí en el inspector
    public bool isPlaceholder;

    public void Setup(string content, bool placeholder) {
        if (textMesh != null) {
            textMesh.text = content;
        }
        isPlaceholder = placeholder;
        if (isPlaceholder) gameObject.tag = "SyllableSlot";
    }
}