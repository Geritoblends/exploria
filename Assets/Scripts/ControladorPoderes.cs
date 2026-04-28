using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ControladorPoderes : MonoBehaviour {

    public static ControladorPoderes instancia;

    [Header("Duraciones")]
    public float duracionEscudo = 10f;
    public float duracionMultiGemas = 30f;
    public float duracionMultiPuntaje = 30f;
    public float duracionSuperSalto = 30f;
    public float duracionIman = 30f;

    // ── Estado interno ──────────────────────────────────
    private bool escudoActivo = false;
    private int  multiplicadorGemas = 1;
    private int  multiplicadorPuntaje = 1;

    private bool superSaltoActivo = false;
    private bool imanActivo = false;

    // ── Poderes de Mascotas (Permanentes) ───────────────
    private bool petIman = false;
    private int petMultiGemas = 1;
    private int petMultiPuntaje = 1;
    private bool petSuperSalto = false;
    private float petReduccionVelocidad = 1f;

    private float temporizadorGemas = 0f;
    private float temporadorPuntaje = 0f;
    private float temporizadorSalto = 0f;
    private float temporizadorIman = 0f;

    // ── Esferas visuales ────────────────────────────────
    private GameObject esferaEscudo;
    private GameObject esferaMultiGemas;
    private GameObject esferaMultiPuntaje;
    private GameObject circuloMascota;

    private List<string> poderesActivadosEnEstaPartida = new List<string>();

    // ── Propiedades públicas ────────────────────────────
    public bool EscudoActivo => escudoActivo;
    public int MultiplicadorGemas => Mathf.Max(multiplicadorGemas, petMultiGemas);
    public int MultiplicadorPuntaje => Mathf.Max(multiplicadorPuntaje, petMultiPuntaje);
    public bool SuperSaltoActivo => superSaltoActivo || petSuperSalto;
    public bool ImanActivo => imanActivo || petIman;
    public float ReduccionVelocidad => petReduccionVelocidad;

    public string[] GetPoderesActivados() {
        string[] array = poderesActivadosEnEstaPartida.ToArray();
        return array;
    }

    public void LimpiarPoderesActivados() {
        poderesActivadosEnEstaPartida.Clear();
    }

    void Awake() {
        if (instancia == null) {
            instancia = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void ActivarPoderMascota(string nombreMascota, Transform transformMascota) {
        switch (nombreMascota) {
            case "Tigrito":
                petMultiGemas = 3;
                break;
            case "Pinguino":
            case "Kikiriki":
                petIman = true;
                break;
            case "Gato":
                petSuperSalto = true;
                break;
            case "Perro":
                petReduccionVelocidad = 0.85f; // Reduce la velocidad un 15%
                break;
            case "Bambi":
                petMultiPuntaje = 3;
                break;
        }

        if (nombreMascota != "Ninguna") {
            CrearCirculoMascota(transformMascota);
        }
    }

    void Update() {
        if (GameManager.instance != null && !GameManager.instance.isGameActive) return;

        // ── Teclas ────────────
        if (Keyboard.current != null) {
            if (Keyboard.current.gKey.wasPressedThisFrame) IntentarActivar("Escudo");
            if (Keyboard.current.uKey.wasPressedThisFrame) IntentarActivar("MultiScore");
            if (Keyboard.current.iKey.wasPressedThisFrame) IntentarActivar("MultiGemas");
        }

        // ── Temporizadores ────────────

        // Gemas
        if (multiplicadorGemas > 1) {
            temporizadorGemas -= Time.deltaTime;
            if (temporizadorGemas <= 0f) {
                multiplicadorGemas = 1;
                if (esferaMultiGemas != null) Destroy(esferaMultiGemas);
            }
        }

        // Puntaje
        if (multiplicadorPuntaje > 1) {
            temporadorPuntaje -= Time.deltaTime;
            if (temporadorPuntaje <= 0f) {
                multiplicadorPuntaje = 1;
                if (esferaMultiPuntaje != null) Destroy(esferaMultiPuntaje);
            }
        }

        // Super salto
        if (superSaltoActivo) {
            temporizadorSalto -= Time.deltaTime;
            if (temporizadorSalto <= 0f) {
                superSaltoActivo = false;
            }
        }

        // Imán
        if (imanActivo) {
            temporizadorIman -= Time.deltaTime;
            if (temporizadorIman <= 0f) {
                imanActivo = false;
            }
        }
    }

    // ── Activación ────────────

    public void IntentarActivar(string nombrePoder) {

        if (nombrePoder == "Escudo" && escudoActivo) return;
        if (nombrePoder == "MultiScore" && multiplicadorPuntaje > 1) return;
        if (nombrePoder == "MultiGemas" && multiplicadorGemas > 1) return;
        if (nombrePoder == "SuperSalto" && superSaltoActivo) return;
        if (nombrePoder == "Iman" && imanActivo) return;

        int cantidad = PlayerPrefs.GetInt("Poder_" + nombrePoder, 0);
        if (cantidad <= 0) return;

        PlayerPrefs.SetInt("Poder_" + nombrePoder, cantidad - 1);
        PlayerPrefs.Save();

        poderesActivadosEnEstaPartida.Add(nombrePoder);

        switch (nombrePoder) {
            case "Escudo": ActivarEscudo(); break;
            case "MultiScore": ActivarMultiPuntaje(); break;
            case "MultiGemas": ActivarMultiGemas(); break;
            case "SuperSalto": ActivarSuperSalto(); break;
            case "Iman": ActivarIman(); break;
        }

        // Actualizar UI
        ManejadorPanelPoderes panel = Object.FindFirstObjectByType<ManejadorPanelPoderes>();
        if (panel != null) panel.ActualizarBotones();
    }

    // ── Poderes ────────────

    public void ActivarEscudo() {
        escudoActivo = true;
        CrearEsferaEscudo();
        StartCoroutine(EscudoRoutine());
    }

    private System.Collections.IEnumerator EscudoRoutine() {
        float timer = duracionEscudo;
        while (timer > 0) {
            if (GameManager.instance == null || GameManager.instance.isGameActive) {
                timer -= Time.deltaTime;
            }
            yield return null;
        }
        DesactivarEscudo();
    }

    private void DesactivarEscudo() {
        escudoActivo = false;
        if (esferaEscudo != null) Destroy(esferaEscudo);
    }

    public void ActivarMultiGemas() {
        multiplicadorGemas = 2;
        temporizadorGemas = duracionMultiGemas;
        CrearEsferaPoder(ref esferaMultiGemas, new Color(0.6f, 0.2f, 0.8f, 0.25f));
    }

    public void ActivarMultiPuntaje() {
        multiplicadorPuntaje = 2;
        temporadorPuntaje = duracionMultiPuntaje;
        CrearEsferaPoder(ref esferaMultiPuntaje, new Color(0.9f, 0.8f, 0.1f, 0.25f));
    }

    public void ActivarSuperSalto() {
        superSaltoActivo = true;
        temporizadorSalto = duracionSuperSalto;
    }

    public void ActivarIman() {
        imanActivo = true;
        temporizadorIman = duracionIman;
    }

    // ── Visuales ────────────

    private void CrearEsferaEscudo() {
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player == null) return;

        esferaEscudo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        esferaEscudo.transform.SetParent(player.transform);
        esferaEscudo.transform.localPosition = Vector3.up * 1f;
        esferaEscudo.transform.localScale = Vector3.one * 2.2f;

        Renderer rend = esferaEscudo.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.4f, 0.4f, 0.4f, 0.25f);
        mat.SetFloat("_Surface", 1);
        mat.SetFloat("_SrcBlend", 5);
        mat.SetFloat("_DstBlend", 10);
        mat.SetFloat("_ZWrite", 0);
        rend.material = mat;

        Destroy(esferaEscudo.GetComponent<Collider>());
    }

    private void CrearEsferaPoder(ref GameObject esferaRef, Color color) {
        if (esferaRef != null) Destroy(esferaRef);

        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player == null) return;

        esferaRef = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        esferaRef.transform.SetParent(player.transform);
        esferaRef.transform.localPosition = Vector3.up * 1f;
        esferaRef.transform.localScale = Vector3.one * 2.2f;

        Renderer rend = esferaRef.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.SetFloat("_Surface", 1);
        mat.SetFloat("_SrcBlend", 5);
        mat.SetFloat("_DstBlend", 10);
        mat.SetFloat("_ZWrite", 0);
        rend.material = mat;

        Destroy(esferaRef.GetComponent<Collider>());
    }

    private void CrearCirculoMascota(Transform mascotaTransform) {
        if (circuloMascota != null) Destroy(circuloMascota);

        circuloMascota = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        circuloMascota.transform.SetParent(mascotaTransform);
        
        // Elevamos la esfera para que no choque con el suelo (asumiendo pivote en los pies)
        circuloMascota.transform.localPosition = Vector3.up * 0.8f; 
        // Aumentamos el tamaño para que no choque con el cuerpo de la mascota
        circuloMascota.transform.localScale = Vector3.one * 2.5f;

        Renderer rend = circuloMascota.GetComponent<Renderer>();
        // Usamos Unlit para que el poder siempre brille y no tenga sombras raras
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        
        mat.color = new Color(0f, 1f, 1f, 0.15f); // Cian suave
        
        // Configuraciones para transparencia real en URP
        mat.SetFloat("_Surface", 1); // 1 = Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000; // Transparent queue

        rend.material = mat;

        // Quitamos el colisionador para que no estorbe físicamente
        Destroy(circuloMascota.GetComponent<Collider>());
    }
}
