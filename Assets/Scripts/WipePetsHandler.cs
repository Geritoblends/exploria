using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class WipePetsHandler : MonoBehaviour {
    
    // List of pets to wipe (matches BackendManager)
    private string[] pets = { "Tigrito", "Pinguino", "Gato", "Pajaro", "Kikiriki", "Perro", "Bambi" };

    public void WipeAllPets() {
        Debug.Log("[WipePetsHandler] Starting local pet wipe...");

        foreach (string pet in pets) {
            string key = "DuenioDe_" + pet;
            if (PlayerPrefs.HasKey(key)) {
                PlayerPrefs.DeleteKey(key);
                Debug.Log($"Deleted ownership for: {pet}");
            }
        }

        // Also reset the equipped pet
        PlayerPrefs.SetString("MascotaEquipada", "Ninguna");
        PlayerPrefs.Save();

        Debug.Log("[WipePetsHandler] Wipe complete. Returning to Main menu.");
        
        // Optional: Reload the main scene to see changes
        SceneManager.LoadScene("Main");
    }

    public void BackToMenu() {
        SceneManager.LoadScene("Main");
    }
}
