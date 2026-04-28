using UnityEngine;
using TMPro;

public class ControladorGemas : MonoBehaviour {
    public static ControladorGemas instancia;
    public int gemasDeEstaPartida; 
    public TextMeshProUGUI textoGemasHUD; 

    void Awake() {
        if (instancia == null) {
            instancia = this;
            // IMPORTANTE: Esto evita que el contador se borre al cambiar de escena 
            // hasta que nosotros le digamos.
            DontDestroyOnLoad(gameObject); 
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        // Si NO estamos reviviendo, significa que es una partida nueva
        if (BackendManager.instance != null && !BackendManager.instance.pendingRevive) {
            gemasDeEstaPartida = 0;
            if (textoGemasHUD != null) textoGemasHUD.text = "0";
        }
    }

    public void SumarGema() {
        int multiplicador = 1;
        if (ControladorPoderes.instancia != null) {
            multiplicador = ControladorPoderes.instancia.MultiplicadorGemas;
        }

        gemasDeEstaPartida += multiplicador;

        if (textoGemasHUD != null) {
            textoGemasHUD.text = gemasDeEstaPartida.ToString();
        }
    }

    public void RestaurarGemas(int cantidad) {
        gemasDeEstaPartida = cantidad;
        if (textoGemasHUD != null) {
            textoGemasHUD.text = gemasDeEstaPartida.ToString();
        }
    }

    // ESTA ES LA FUNCIÓN MÁGICA NUEVA
    // Se ejecuta automáticamente cuando el objeto se destruye (al cerrar el juego o cambiar de nivel)
    void OnDisable() {
        // Si estamos reviviendo, no guardamos las gemas todavía porque la partida continúa
        if (BackendManager.instance != null && BackendManager.instance.pendingRevive) {
            return;
        }

        int gemasGuardadas = PlayerPrefs.GetInt("GemasTotales", 0);
        PlayerPrefs.SetInt("GemasTotales", gemasGuardadas + gemasDeEstaPartida);
        PlayerPrefs.Save();
        Debug.Log("Gemas guardadas en la caja fuerte: " + (gemasGuardadas + gemasDeEstaPartida));
    }
}