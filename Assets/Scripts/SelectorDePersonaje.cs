using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectorPersonaje : MonoBehaviour {

    public void ElegirPersonaje(string nombre) {
        // Guardamos en la memoria quién fue el elegido
        PlayerPrefs.SetString("PersonajeElegido", nombre);
        PlayerPrefs.Save();

        Debug.Log("Elegiste a: " + nombre);

        // Te lleva a la escena de Login (cambia el nombre por el real de tu escena)
        SceneManager.LoadScene("LoginScene");
    }
}