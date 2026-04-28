using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class StickerUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject stickerPanel;
    public Image stickerImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI daysText;

    [Header("Auto-fetch on Start")]
    public bool fetchOnStart = false;

    void Start()
    {
        if (stickerPanel != null) stickerPanel.SetActive(false);
        
        if (fetchOnStart)
        {
            FetchAndShowSticker();
        }
    }

    public void FetchAndShowSticker()
    {
        if (BackendManager.instance == null)
        {
            Debug.LogError("[StickerUI] BackendManager instance not found!");
            return;
        }

        Debug.Log("[StickerUI] Fetching last sticker...");
        BackendManager.instance.GetLastSticker(
            (sticker) => {
                BackendManager.instance.lastSticker = sticker; // Actualizamos el cache global
                ShowSticker(sticker);
                
                // Si hay un display de racha en la escena, lo actualizamos
                RachaDisplay rd = Object.FindFirstObjectByType<RachaDisplay>();
                if (rd != null) rd.UpdateDisplay();
            },
            (error) => {
                Debug.LogWarning("[StickerUI] Sticker fetch failed: " + error);
                if (stickerPanel != null) stickerPanel.SetActive(false);
            }
        );
    }

    private void ShowSticker(BackendManager.StickerResponse sticker)
    {
        if (stickerPanel != null) stickerPanel.SetActive(true);
        if (nameText != null) nameText.text = sticker.name;
        if (descriptionText != null) descriptionText.text = sticker.description;
        if (daysText != null) daysText.text = sticker.sticker_days + " días";

        if (stickerImage != null && !string.IsNullOrEmpty(sticker.image))
        {
            StartCoroutine(LoadImage(sticker.image));
        }
    }

    IEnumerator LoadImage(string url)
    {
        Debug.Log("[StickerUI] Loading image from: " + url);
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (stickerImage != null)
                {
                    stickerImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
            else
            {
                Debug.LogError("[StickerUI] Error loading sticker image: " + request.error);
            }
        }
    }

    public void ClosePanel()
    {
        if (stickerPanel != null) stickerPanel.SetActive(false);
    }
}
