using UnityEngine;

public class IniciadorMusica : MonoBehaviour {
    public string nombreDeEstaCueva; // Aquí escribirás "Tierra", "Fuego", etc.

    void Start() {
        // Le avisamos al AudioManager qué música poner al entrar a la escena
        if (AudioManager.instancia != null) {
            AudioManager.instancia.CambiarMusica(nombreDeEstaCueva);
        }
    }
}