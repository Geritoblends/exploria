using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SelectorDePoderes : MonoBehaviour {
    public string nombrePoder; // Escribe "Escudo", "MultiGemas" o "MultiPuntaje"
    public Button botonUsar;
    public TextMeshProUGUI textoEstado;

    void Awake() {
        // AUTO-CONEXIÓN: Busca los componentes en el mismo objeto si no están asignados
        if (botonUsar == null) botonUsar = GetComponent<Button>();
        if (textoEstado == null) textoEstado = GetComponentInChildren<TextMeshProUGUI>();

        // AUTO-EVENTO: Configura el click del botón por código
        if (botonUsar != null) {
            botonUsar.onClick.RemoveAllListeners();
            botonUsar.onClick.AddListener(PresionarUsar);
        }
    }

    void Start() {
        ActualizarInterfaz();
    }

    public void ActualizarInterfaz() {
        int cantidad = PlayerPrefs.GetInt("Poder_" + nombrePoder, 0);

        textoEstado.text = "x" + cantidad;
        botonUsar.interactable = cantidad > 0;
        
    }

    public void PresionarUsar() {
        if (ControladorPoderes.instancia != null) {
            ControladorPoderes.instancia.IntentarActivar(nombrePoder);
        }

        // Actualizar todos los botones del panel para refrescar stock
        ManejadorPanelPoderes manejador = Object.FindFirstObjectByType<ManejadorPanelPoderes>();
        if (manejador != null) {
            manejador.ActualizarBotones();
            // Opcional: manejador.CerrarPanel(); // Descomenta si quieres que se cierre al usar
        } else {
            // Fallback: si no hay manejador, actualizar todos los selectores sueltos
            foreach (var sp in Object.FindObjectsByType<SelectorDePoderes>(FindObjectsSortMode.None)) {
                sp.ActualizarInterfaz();
            }
        }
    }
}

