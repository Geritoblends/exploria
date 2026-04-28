using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SelectorMascotas : MonoBehaviour {
    public string nombreMascota; // Escribe "Gato" en el Inspector
    public Button botonEquipar;
    public TextMeshProUGUI textoEstado; // Un texto que diga "Bloqueado", "Equipar" o "Equipado"
    public TextMeshProUGUI textoPoder; // NUEVO: Arrastra aquí el texto que muestra el poder en la cajita

    void Start() {
        ActualizarInterfaz();
    }

    public void ActualizarInterfaz() {
    int usos;
    if (nombreMascota == "Ninguna") {
        usos = 999; // Infinito para "Ninguna"
    } else {
        usos = PlayerPrefs.GetInt("UsosDe_" + nombreMascota, 0);
    }

    // Actualizar texto de poder si existe la referencia
    if (textoPoder != null) {
        textoPoder.text = ObtenerPoderTexto(nombreMascota);
    }

    if (usos > 0) {
        string equipada = PlayerPrefs.GetString("MascotaEquipada", "Ninguna");
        
        if (equipada == nombreMascota) {
            if (nombreMascota == "Ninguna") {
                textoEstado.text = "SIN MASCOTA";
            } else {
                textoEstado.text = "EQUIPADO";
            }
            botonEquipar.interactable = false;
        } else {
            if (nombreMascota == "Ninguna") {
                textoEstado.text = "DESEQUIPAR";
            } else {
                // MOSTRAR CANTIDAD DE USOS DISPONIBLES
                textoEstado.text = "x" + usos + " EQUIPAR";
            }
            botonEquipar.interactable = true;
        }
    } else {
        textoEstado.text = "BLOQUEADO"; // O "IR A TIENDA"
        botonEquipar.interactable = false;
    }
}

    private string ObtenerPoderTexto(string nombre) {
        switch (nombre) {
            case "Tigrito": return "x3 Gemas";
            case "Pinguino": return "Imán";
            case "Gato": return "Súper Salto";
            case "Kikiriki": return "Imán";
            case "Perro": return "Bajar Velocidad";
            case "Bambi": return "x3 Score";
            default: return "";
        }
    }

    public void PresionarEquipar() {
        // Guardamos la mascota elegida
        PlayerPrefs.SetString("MascotaEquipada", nombreMascota);
        PlayerPrefs.Save();
        
        // Avisamos a todos los botones que se actualicen (para que el otro diga "Equipar" y este "Equipado")
        Object.FindObjectsByType<SelectorMascotas>(FindObjectsSortMode.None);
        foreach(SelectorMascotas sm in Object.FindObjectsByType<SelectorMascotas>(FindObjectsSortMode.None)) {
            sm.ActualizarInterfaz();
        }
    }
}
