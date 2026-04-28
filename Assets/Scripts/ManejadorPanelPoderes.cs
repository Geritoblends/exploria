using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Maneja la visibilidad del popover de poderes y la actualización de sus botones.
/// </summary>
public class ManejadorPanelPoderes : MonoBehaviour {
    
    // Singleton simple para que el GameManager lo encuentre aunque esté desactivado
    public static ManejadorPanelPoderes instancia;

    [Header("UI Panels")]
    public GameObject panelPopover; // El panel que contiene los botones de poderes

    [Header("Botones de Poderes")]
    public SelectorDePoderes[] selectores; 

    void Awake() {
        instancia = this;

        // AUTO-CONEXIÓN: Busca todos los botones de poderes que sean hijos de este panel
        if (selectores == null || selectores.Length == 0) {
            selectores = GetComponentsInChildren<SelectorDePoderes>(true);
        }

        // Si no asignaste el panel, asumimos que es este mismo objeto
        if (panelPopover == null) {
            panelPopover = this.gameObject;
        }
    }

    void Start() {
        if (panelPopover != null) {
            panelPopover.SetActive(true);
        }
    }

    /// <summary>
    /// Abre o cierra el panel
    /// </summary>
    public void TogglePanel() {
        if (panelPopover == null) return;

        bool estaActivo = !panelPopover.activeSelf;
        panelPopover.SetActive(estaActivo);

        if (estaActivo) {
            ActualizarBotones();
        }
    }

    public void ActualizarBotones() {
        foreach (var selector in selectores) {
            if (selector != null) {
                selector.ActualizarInterfaz();
            }
        }
    }

    public void CerrarPanel() {
        if (panelPopover != null) {
            panelPopover.SetActive(false);
        }
    }
}
