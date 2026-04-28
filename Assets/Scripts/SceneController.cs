using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Referencias de Texto UI")]
    public TextMeshProUGUI textoPuntaje; 
    public TextMeshProUGUI textoRecord;  

    [Header("Escribe el nombre EXACTO de tu menú")]
    [Tooltip("Ejemplo: Main (Respeta mayúsculas)")]
    public string nombreEscenaMenu = "Main"; 

    void Start()
    {
        // 1. Recuperamos los datos guardados en el GameManager
        int ultimoPuntaje = PlayerPrefs.GetInt("UltimoPuntaje", 0);
        int mejorRecord = PlayerPrefs.GetInt("Highscore", 0);

        // 2. Mostramos los datos
        if (textoPuntaje != null)
            textoPuntaje.text = "Score: " + ultimoPuntaje + "m";

        if (textoRecord != null)
            textoRecord.text = "Record: " + mejorRecord + "m";
    }

    // Función para el botón de "Main Menu"
    public void IrAlMenu()
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
            Debug.LogError("¡No has escrito el nombre de la escena del MENU en el Inspector!");
        }
    }

    // --- NUEVAS FUNCIONES PARA EL BACKEND ---
    
    public void Revivir() {
        if (BackendManager.instance != null) {
            BackendManager.instance.PublicRevive();
        } else {
            Debug.LogError("No se encontró BackendManager.instance para revivir.");
        }
    }

    public void CanjearMonedasPorGemas(int cantidad) {
        if (BackendManager.instance != null) {
            BackendManager.instance.PublicExchangeCoinsForGems(cantidad);
        } else {
            Debug.LogError("No se encontró BackendManager.instance para canjear.");
        }
    }
}