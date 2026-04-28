using UnityEngine;

public class MascotaSeguidora : MonoBehaviour {
    public string nombreMascota; // Escribe "Gato"
    public Transform objetivo; // Arrastra aquí el "PuntoMascota"
    public float velocidad = 5.0f;

    void Start() {
        // Al empezar el nivel, preguntamos: ¿Esta mascota es la que está equipada?
        string equipada = PlayerPrefs.GetString("MascotaEquipada", "Ninguna");

        if (equipada == nombreMascota) {
            // VERIFICAR USOS
            int usos = PlayerPrefs.GetInt("UsosDe_" + nombreMascota, 0);
            
            if (usos > 0) {
                gameObject.SetActive(true); // Se muestra
                
                // RESTAR UN USO
                usos--;
                PlayerPrefs.SetInt("UsosDe_" + nombreMascota, usos);
                
                // Si se acabaron los usos, la desequipamos para la próxima vez
                if (usos <= 0) {
                    PlayerPrefs.SetString("MascotaEquipada", "Ninguna");
                    Debug.Log("¡Se agotaron los usos de " + nombreMascota + "!");
                }
                PlayerPrefs.Save();

                // Si hay un controlador de poderes, activamos su habilidad
                if (ControladorPoderes.instancia != null) {
                    ControladorPoderes.instancia.ActivarPoderMascota(nombreMascota, transform);
                }
            } else {
                gameObject.SetActive(false); // No tiene usos, no sale
            }
        } else {
            gameObject.SetActive(false); // Se apaga
        }
    }

    void Update() {
        if (GameManager.instance != null && !GameManager.instance.isGameActive) return;

        // Si está encendida, sigue al punto con suavidad
        if (objetivo != null) {
            transform.position = Vector3.Lerp(transform.position, objetivo.position, velocidad * Time.deltaTime);
            // Que siempre mire hacia adelante
            transform.rotation = Quaternion.Lerp(transform.rotation, objetivo.rotation, velocidad * Time.deltaTime);
        }
    }
}