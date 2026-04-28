using UnityEngine;
using TMPro;

public class ManejadorTienda : MonoBehaviour {
    public TextMeshProUGUI textoGemasTienda; // Para actualizar el número al comprar

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

    // Ahora la función recibe NOMBRE y PRECIO desde el botón
    public void IntentarComprarMascota(string datos) {
        Debug.Log("[ManejadorTienda] Clicked purchase with data: " + datos);
        string[] partes = datos.Split(',');
        string nombreMascota = partes[0];
        int precio = int.Parse(partes[1]);

        int gemasActuales = PlayerPrefs.GetInt("GemasTotales", 0);

        // 1. ¿Tiene usos disponibles? (Si tiene, no puede comprar más hasta agotarlos)
        int usosRestantes = PlayerPrefs.GetInt("UsosDe_" + nombreMascota, 0);
        if (usosRestantes > 0) {
            Debug.Log("Ya tienes a " + nombreMascota + " con " + usosRestantes + " usos.");
            MensajesUI.instancia.Mostrar(MensajesUI.instancia.imgAviso);
            return;
        }

        // 2. ¿Tiene dinero?
        if (gemasActuales >= precio) {
            if (BackendManager.instance != null) {
                BackendManager.instance.PurchasePet(nombreMascota, precio, (success) => {
                    if (success) {
                        ConfirmarCompra(nombreMascota, precio);
                    } else {
                        MensajesUI.instancia.Mostrar(MensajesUI.instancia.imgError);
                    }
                });
            } else {
                ConfirmarCompra(nombreMascota, precio);
            }
        } else {
            MensajesUI.instancia.Mostrar(MensajesUI.instancia.imgError);
        }
    }

    private void ConfirmarCompra(string nombreMascota, int precio) {
        int gemasActuales = PlayerPrefs.GetInt("GemasTotales", 0);
        gemasActuales -= precio;
        PlayerPrefs.SetInt("GemasTotales", gemasActuales);
        
        // DAMOS 3 USOS
        PlayerPrefs.SetInt("UsosDe_" + nombreMascota, 3);
        PlayerPrefs.Save();

        if (textoGemasTienda != null) textoGemasTienda.text = gemasActuales.ToString();

        Debug.Log("Compraste " + nombreMascota + " (3 usos) por " + precio);
        MensajesUI.instancia.Mostrar(MensajesUI.instancia.imgExito);
    }

    // Función útil para mostrar el poder en la UI de la tienda
    public string ObtenerPoderTexto(string nombre) {
        switch (nombre) {
            case "Tigrito": return "Poder: x3 Gemas";
            case "Pinguino": return "Poder: Imán";
            case "Gato": return "Poder: Súper Salto";
            case "Kikiriki": return "Poder: Imán";
            case "Perro": return "Poder: Bajar Velocidad";
            case "Bambi": return "Poder: x3 Score";
            default: return "Sin Poder";
        }
    }
}
