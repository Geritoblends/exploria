using UnityEngine;
using UnityEngine.EventSystems;

// Este script no depende de botones, solo detecta si el mouse toca este objeto
public class BotonSonidoIndependiente : MonoBehaviour, IPointerDownHandler {

    public void OnPointerDown(PointerEventData eventData) {
        // Llamamos directamente al sonido
        if (AudioManager.instancia != null) {
            AudioManager.instancia.PlayBoton();
            Debug.Log(">>> SONIDO FORZADO DESDE SCRIPT INDEPENDIENTE <<<");
        }
    }
}