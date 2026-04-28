using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChequeoPoder : MonoBehaviour {
    public string nombrePoder;
    public TextMeshProUGUI textoBoton;
    public Button miBoton;

    void Start() {
        ActualizarEstado();
    }

    public void ActualizarEstado() {
        int cantidad = PlayerPrefs.GetInt("Poder_" + nombrePoder, 0);

        if (cantidad > 0) {
            textoBoton.text = "x" + cantidad.ToString();
            miBoton.interactable = true;
        } else {
            textoBoton.text = "COMPRAR";
            miBoton.interactable = true;
        }
    }
}

