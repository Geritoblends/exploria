using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SelectorDeCuevas : MonoBehaviour
{
    [Header("Escribe el nombre EXACTO de tus escenas")]
    [Tooltip("Ejemplo: Cueva1, Cueva2... (Respeta mayúsculas)")]
    public List<string> nombresDeCuevas; 

    public void CargarNivel(int indice)
    {
        if (indice >= 0 && indice < nombresDeCuevas.Count)
        {
            // Verificamos que no esté vacío el texto
            if (!string.IsNullOrEmpty(nombresDeCuevas[indice]))
            {
                SceneManager.LoadScene(nombresDeCuevas[indice]);
            }
            else
            {
                Debug.LogError("El nombre de la escena está vacío en el Inspector.");
            }
        }
        else
        {
            Debug.LogError("¡Ese índice no existe en la lista!");
        }
    }
}