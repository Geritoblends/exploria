using UnityEngine;
using TMPro;

public class RachaDisplay : MonoBehaviour
{
    public TextMeshProUGUI rachaText;
    public string prefix = "Racha: ";

    void Start()
    {
        if (rachaText == null) rachaText = GetComponent<TextMeshProUGUI>();
        
        UpdateDisplay();

        if (BackendManager.instance != null)
        {
            BackendManager.instance.OnDataSynced += UpdateDisplay;
            
            // If we don't have a sticker yet, try to fetch it
            if (BackendManager.instance.lastSticker == null)
            {
                BackendManager.instance.GetLastSticker(
                    (sticker) => {
                        // UpdateDisplay is called via event
                    },
                    (error) => {
                        if (rachaText != null) rachaText.text = ""; 
                    }
                );
            }
        }
    }

    void OnDestroy()
    {
        if (BackendManager.instance != null)
        {
            BackendManager.instance.OnDataSynced -= UpdateDisplay;
        }
    }

    public void UpdateDisplay()
    {
        if (rachaText == null) return;

        if (BackendManager.instance != null && BackendManager.instance.lastSticker != null)
        {
            rachaText.text = prefix + BackendManager.instance.lastSticker.sticker_days;
        }
        else
        {
            // Default or empty if not linked
            rachaText.text = ""; 
        }
    }
}
