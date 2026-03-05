using UnityEngine;

public class ManagerLetra : MonoBehaviour
{
    public GameObject[] letterPrefabs; // Aquí van A-Z

    private GameObject currentLetter;

    public void ShowLetter(int index)
    {
        // Borrar la letra anterior si existe
        if (currentLetter != null)
        {
            Destroy(currentLetter);
        }

        // Instanciar la nueva
        currentLetter = Instantiate(letterPrefabs[index], Vector3.zero, Quaternion.identity);
    }
}
