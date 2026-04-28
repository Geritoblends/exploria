using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ActualizadorTienda : MonoBehaviour {
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

    void Update() {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame) {
            int total = PlayerPrefs.GetInt("GemasTotales", 0) + 500;
            PlayerPrefs.SetInt("GemasTotales", total);
            PlayerPrefs.Save();

            if (textoGemasTienda != null) textoGemasTienda.text = total.ToString();
            Debug.Log("+500 gems! Total: " + total);
        }
    }
}

