using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChequeoCompra : MonoBehaviour {
    public string nombreMascota;
    public TextMeshProUGUI textoBoton;
    public Button miBoton;

    void Start() {
        // Al entrar a la tienda, revisamos si ya la compró antes
        if (PlayerPrefs.GetInt("DuenioDe_" + nombreMascota, 0) == 1) {
            textoBoton.text = "Comprado";
            miBoton.interactable = false; // Desactivar el botón para no gastar de más
        }
    }
}