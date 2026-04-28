using UnityEngine;

public class ActivadorPersonaje : MonoBehaviour {
    public GameObject modeloNino;
    public GameObject modeloNina;

    void Awake() {
        // Leemos la memoria
        string elegido = PlayerPrefs.GetString("PersonajeElegido", "Niño"); // Niño por defecto

        if (elegido == "Niña") {
            modeloNino.SetActive(false);
            modeloNina.SetActive(true);
        } else {
            modeloNino.SetActive(true);
            modeloNina.SetActive(false);
        }
    }
}