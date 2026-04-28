using UnityEngine;
using UnityEngine.UI;

public class MensajesUI : MonoBehaviour
{
    public static MensajesUI instancia;

    public GameObject panel;
    public Image imagen;

    public Sprite imgError;
    public Sprite imgExito;
    public Sprite imgAviso;

    void Awake()
    {
        instancia = this;
        panel.SetActive(false);
    }

    public void Mostrar(Sprite sprite)
    {
        Debug.Log("[MensajesUI] Mostrar called with sprite: " + (sprite != null ? sprite.name : "NULL"));
        if (panel != null)
        {
            imagen.sprite = sprite;
            panel.SetActive(true);
            Debug.Log("[MensajesUI] Panel activated: " + panel.name);
        }
        else
        {
            Debug.LogError("[MensajesUI] Panel reference is missing!");
        }

        CancelInvoke();
        Invoke("Ocultar", 2f);
    }

    void Ocultar()
    {
        panel.SetActive(false);
    }
}