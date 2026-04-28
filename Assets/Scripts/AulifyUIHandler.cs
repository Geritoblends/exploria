using UnityEngine;
using TMPro;

public class AulifyUIHandler : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Header("Optional Sticker UI")]
    public StickerUI stickerUI;

    /// <summary>
    /// This function should be called by the OnClick event of your "Link" button.
    /// It grabs the strings from the input fields and sends them to the BackendManager.
    /// </summary>
    public void OnClickLinkAulify()
    {
        if (emailInput == null || passwordInput == null)
        {
            Debug.LogError("InputFields not assigned in the AulifyUIHandler Inspector!");
            return;
        }

        string email = emailInput.text;
        string pass = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            Debug.LogWarning("Email or Password cannot be empty.");
            return;
        }

        if (BackendManager.instance != null)
        {
            Debug.Log($"Attempting to link Aulify account: {email}");
            BackendManager.instance.LinkAulify(email, pass, (success, error) => {
                if (success)
                {
                    Debug.Log("Link successful! Fetching sticker...");
                    if (stickerUI != null)
                    {
                        stickerUI.FetchAndShowSticker();
                    }
                }
                else
                {
                    Debug.LogError("Link failed: " + error);
                    // Optionally show an error message to the user here
                }
            });
        }
        else
        {
            Debug.LogError("BackendManager instance not found in scene!");
        }
    }
}
