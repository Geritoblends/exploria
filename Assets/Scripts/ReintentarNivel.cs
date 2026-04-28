using UnityEngine;
using UnityEngine.SceneManagement;

public class ReintentarNivel : MonoBehaviour
{
    void Awake()
    {
        string nombreEscena = SceneManager.GetActiveScene().name;

    
        if (nombreEscena != "Dead")
        {
            PlayerPrefs.SetString("UltimaCueva", nombreEscena);
            Debug.Log("Guardando cueva: " + nombreEscena);
        }
    }

    public void Reintentar()
    {
        // Enviamos los datos antes de reiniciar la partida
        if (BackendManager.instance != null) {
            BackendManager.instance.SendFinalRunRecord();
        }

        string escena = PlayerPrefs.GetString("UltimaCueva", "");

        Debug.Log("Cargando: " + escena);

        if (!string.IsNullOrEmpty(escena))
        {
            SceneManager.LoadScene(escena);
        }
        else
        {
            Debug.LogError("No hay cueva guardada");
        }
    }
}