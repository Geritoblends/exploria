using UnityEngine;
using System.Collections.Generic;

public class BackendWiper : MonoBehaviour
{
    [Header("Settings")]
    public bool wipeOnStart = true;
    public bool debugLog = true;

    private static readonly string[] StaticKeys = {
        "session_token",
        "OfflineQueue",
        "GemasTotales",
        "UltimoPuntaje",
        "UltimasGemas",
        "Highscore",
        "MascotaEquipada",
        "LastStickerId",
        "LastStickerDays",
        "UltimaCueva"
    };

    private static readonly string[] PowerNames = {
        "Iman", "Magnet", "SuperJump", "SuperSalto", "Super Salto",
        "MultiScore", "ScoreMulti", "MultiPuntaje", "MultiGemas",
        "GemMulti", "Escudo", "Shield"
    };

    private static readonly string[] PetNames = {
        "Tigrito", "Pinguino", "Gato", "Pajaro", "Kikiriki", "Perro", "Bambi"
    };

    void Start()
    {
        if (wipeOnStart)
        {
            WipeBackendData();
        }
    }

    [ContextMenu("Wipe Backend Data Now")]
    public void WipeBackendData()
    {
        if (debugLog) Debug.Log("<color=red>[BackendWiper]</color> Starting wipe of backend-related PlayerPrefs...");

        int count = 0;

        // Wipe Static Keys
        foreach (string key in StaticKeys)
        {
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                count++;
                if (debugLog) Debug.Log($"Deleted key: {key}");
            }
        }

        // Wipe Powers
        foreach (string power in PowerNames)
        {
            string key = "Poder_" + power;
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                count++;
                if (debugLog) Debug.Log($"Deleted key: {key}");
            }
        }

        // Wipe Pets
        foreach (string pet in PetNames)
        {
            string key = "DuenioDe_" + pet;
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                count++;
                if (debugLog) Debug.Log($"Deleted key: {key}");
            }
        }

        PlayerPrefs.Save();
        
        if (debugLog) Debug.Log($"<color=green>[BackendWiper]</color> Wipe complete. Total keys deleted: {count}");
    }
}

