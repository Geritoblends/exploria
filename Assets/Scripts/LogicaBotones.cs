using UnityEngine;
using UnityEngine.SceneManagement;

public class LogicaBotones : MonoBehaviour
{
    [Header("Screens (UI)")]
    public GameObject[] screens;

    [Header("Scenes (niveles)")]
    public string[] escenas;


    public void CambiarPantalla(int index)
    {
        if (index < 0 || index >= screens.Length)
        {
            Debug.LogError("Índice de pantalla inválido");
            return;
        }

        for (int i = 0; i < screens.Length; i++)
        {
            screens[i].SetActive(i == index);
        }
    }


    public void CargarEscena(int index)
    {
        if (index < 0 || index >= escenas.Length)
        {
            Debug.LogError("Índice de escena inválido");
            return;
        }

        // Si salimos de una partida, enviamos los datos acumulados
        if (BackendManager.instance != null) {
            BackendManager.instance.SendFinalRunRecord();
        }

        if (!string.IsNullOrEmpty(escenas[index]))
        {
            SceneManager.LoadScene(escenas[index]);
        }
        else
        {
            Debug.LogError("Nombre de escena vacío");
        }
    }

    public void Revivir() {
        if (BackendManager.instance != null) {
            BackendManager.instance.PublicRevive();
        }
    }

    public void CanjearMonedas(int cantidad) {
        if (BackendManager.instance != null) {
            BackendManager.instance.PublicExchangeCoinsForGems(cantidad);
        } else {
            Debug.LogError("No se encontró BackendManager.instance para canjear.");
        }
    }
}