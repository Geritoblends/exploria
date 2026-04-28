using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuSelector : MonoBehaviour
{
    [Header("Botones (imagenes)")]
    public Image[] botones;

    [Header("Textos (opcional)")]
    public TextMeshProUGUI[] textos;

    [Header("Escala")]
    public float escalaNormal = 1f;
    public float escalaSeleccionada = 1.15f;

    [Header("Colores (opcional)")]
    public Color colorNormal = Color.white;
    public Color colorSeleccionado = Color.cyan;

    [Header("Clave única (IMPORTANTE)")]
    public string clave = "MenuSeleccionado";

    int seleccionado;

    void Start()
    {
        seleccionado = PlayerPrefs.GetInt(clave, 0);
        ActualizarBotones();
    }

    public void Seleccionar(int index)
    {
        seleccionado = index;
        PlayerPrefs.SetInt(clave, index);
        ActualizarBotones();
    }

    void ActualizarBotones()
    {
        for (int i = 0; i < botones.Length; i++)
        {
            if (i == seleccionado)
                botones[i].transform.localScale = Vector3.one * escalaSeleccionada;
            else
                botones[i].transform.localScale = Vector3.one * escalaNormal;

            if (textos != null && textos.Length > i)
            {
                if (i == seleccionado)
                    textos[i].color = colorSeleccionado;
                else
                    textos[i].color = colorNormal;
            }
        }
    }
}