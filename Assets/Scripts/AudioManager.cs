using UnityEngine;
using UnityEngine.EventSystems;

public class AudioManager : MonoBehaviour {
    public static AudioManager instancia;
    private AudioSource fuenteEfectos; // Para gemas y botones
    private AudioSource fuenteMusica;  // Para la música de fondo

    [Header("Efectos de Sonido")]
    public AudioClip sonidoGema;
    public AudioClip sonidoBoton;
    public AudioClip sonidoCompra;

    [Header("Música de las Cuevas")]
    public AudioClip musicaTierra;
    public AudioClip musicaAire;
    public AudioClip musicaFuego;
    public AudioClip musicaHielo;
    public AudioClip musicaMenu;

    void Awake() {
        if (instancia == null) {
            instancia = this;
            DontDestroyOnLoad(gameObject);
            
            // Creamos dos fuentes para que la música y los efectos no se corten entre sí
            AudioSource[] fuentes = GetComponents<AudioSource>();
            fuenteEfectos = fuentes[0];
            fuenteMusica = fuentes[1];
            fuenteMusica.loop = true; // La música siempre debe repetirse
        } else {
            Destroy(gameObject);
        }
    }

    // Funciones para efectos (Botones y Gemas)
    public void PlayGema() { fuenteEfectos.PlayOneShot(sonidoGema); }
    public void PlayBoton() {
    // Si tienes un AudioSource solo para efectos, lo usamos directamente
    if (fuenteEfectos != null && sonidoBoton != null) {
        // PlayOneShot es como disparar una flecha; no importa si la anterior sigue volando
        fuenteEfectos.PlayOneShot(sonidoBoton);
        Debug.Log("¡CLICK DETECTADO POR EVENT TRIGGER!");
    }
}
    public void PlayCompra() { fuenteEfectos.PlayOneShot(sonidoCompra); }

    // Función para cambiar la música según la cueva
    public void CambiarMusica(string nombreCueva) {
        AudioClip clipElegido = null;

        switch (nombreCueva) {
            case "Tierra": clipElegido = musicaTierra; break;
            case "Aire":   clipElegido = musicaAire; break;
            case "Fuego":  clipElegido = musicaFuego; break;
            case "Hielo":  clipElegido = musicaHielo; break;
            case "Menu":   clipElegido = musicaMenu; break;
        }

        if (clipElegido != null && fuenteMusica.clip != clipElegido) {
            fuenteMusica.clip = clipElegido;
            fuenteMusica.Play();
        }
    }
}