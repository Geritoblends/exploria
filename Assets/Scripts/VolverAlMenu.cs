using UnityEngine;
using UnityEngine.SceneManagement;

public class VolverAlMenu : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Escribe el nombre exacto de tu escena principal (respeta mayúsculas)")]
    public string nombreEscenaMenu = "Main";

    // Esta es la función que vas a conectar a tu botón
    public void Regresar()
    {
        if (BackendManager.instance != null)
        {
            BackendManager.instance.SendFinalRunRecord();
        }

        if (!string.IsNullOrEmpty(nombreEscenaMenu))
        {
            SceneManager.LoadScene(nombreEscenaMenu);
        }
        else
        {
            Debug.LogError("¡Ojo! No has escrito el nombre de la escena en el Inspector.");
        }
    }
}