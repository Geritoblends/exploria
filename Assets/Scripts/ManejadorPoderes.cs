using UnityEngine;
using TMPro;

public class ManejadorPoderes : MonoBehaviour {
    public TextMeshProUGUI textoGemasTienda;

    void Start() {
        ActualizarTexto();
        if (BackendManager.instance != null) {
            BackendManager.instance.OnDataSynced += ActualizarTexto;
        }
    }

    void OnDestroy() {
        if (BackendManager.instance != null) {
            BackendManager.instance.OnDataSynced -= ActualizarTexto;
        }
    }

    void ActualizarTexto() {
        int total = PlayerPrefs.GetInt("GemasTotales", 0);
        if (textoGemasTienda != null) {
            textoGemasTienda.text = total.ToString();
        }
    }

    // Se usa desde el botón con el formato "Escudo,500"
    public void IntentarComprarPoder(string datos) {
        Debug.Log("[ManejadorPoderes] Clicked purchase with data: " + datos);
        string[] partes = datos.Split(',');
        string nombrePoder = partes[0];
        int precio = int.Parse(partes[1]);

        

        int gemasActuales = PlayerPrefs.GetInt("GemasTotales", 0);

        if (gemasActuales >= precio) {
            // Llamada al Backend
            if (BackendManager.instance != null) {
                BackendManager.instance.PurchaseSuperPower(nombrePoder, precio, (success) => {
                    if (success) {
                        ConfirmarCompra(nombrePoder, precio);
                    } else {
                        MensajesUI.instancia.Mostrar(MensajesUI.instancia.imgError);
                    }
                });
            } else {
                // Fallback local
                ConfirmarCompra(nombrePoder, precio);
            }
        } else {
            Debug.Log("Te faltan gemas para " + nombrePoder);
            MensajesUI.instancia.Mostrar(MensajesUI.instancia.imgError);
        }
    }

    private void ConfirmarCompra(string nombrePoder, int precio) {
        int gemasActuales = PlayerPrefs.GetInt("GemasTotales", 0);
        gemasActuales -= precio;
        PlayerPrefs.SetInt("GemasTotales", gemasActuales);

        int cantidad = PlayerPrefs.GetInt("Poder_" + nombrePoder, 0);
        PlayerPrefs.SetInt("Poder_" + nombrePoder, cantidad + 1);
        PlayerPrefs.Save();

        if (textoGemasTienda != null) 
            textoGemasTienda.text = gemasActuales.ToString();

        Debug.Log("Compraste " + nombrePoder);
        MensajesUI.instancia.Mostrar(MensajesUI.instancia.imgExito);
    }
}