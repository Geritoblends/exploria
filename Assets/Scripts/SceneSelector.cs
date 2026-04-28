using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelector : MonoBehaviour
{
    // Esta variable guardará el nombre, pero la seleccionaremos de una lista
    public string escenaAMostrar;

    public void CambiarEscena()
    {
        if (!string.IsNullOrEmpty(escenaAMostrar))
        {
            SceneManager.LoadScene(escenaAMostrar);
        }
        else
        {
            Debug.LogError("¡No has seleccionado ninguna escena en el Inspector!");
        }
    }
}