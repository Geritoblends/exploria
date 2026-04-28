using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using TMPro; 
using System.Collections.Generic;

public class GameManager : MonoBehaviour 
{
    public static GameManager instance;
    void Awake() { instance = this; }

    [Header("Interfaz de Usuario (UI)")]
    public GameObject[] vidasUI; 
    public TextMeshProUGUI puntajeTexto; 

    [Header("Configuración de la Pista")]
    public GameObject startingChunkPrefab; 
    public GameObject[] chunkPrefabs;
    public float chunkLength = 30f; 
    public float chunkGap = 0f; 
    public int chunksOnScreen = 5;

    [Header("🚄 Control de Velocidad (Sin Arrancones)")]
    [Tooltip("Velocidad inicial del juego")]
    public float velocidadBase = 10f; 
    [Tooltip("Velocidad máxima permitida")]
    public float velocidadMaxima = 40f;
    [Tooltip("Cuánto sube la velocidad por segundo (ej. 0.2 para que sea gradual)")]
    public float aceleracion = 0.2f; 

    [Header("💥 Rebote Suave")]
    [Tooltip("Fuerza del rebote hacia atrás (Debe ser negativo, ej. -10)")]
    public float velocidadRebote = -10f;
    [Tooltip("Cuánto tiempo dura yendo hacia atrás")]
    public float tiempoRebote = 0.5f;
    [Tooltip("Qué tan suave retoma su velocidad normal (1 = lento, 5 = rápido)")]
    public float suavidadRecuperacion = 2f; 

    [Header("Sistema de Vidas")]
    public int maxLives = 3;            
    private int currentLives;
    public string gameOverSceneName = "Dead"; 

    [Header("=== BOTÓN PAUSA ===")]
    public Button pauseButton;
    public GameObject pauseIcon;
    public GameObject playIcon;
    public GameObject pausePanel;

    [Header("=== COUNTDOWN ===")]
    public TextMeshProUGUI countdownText;
    public int countdownSeconds = 3;
    private bool isPaused = false;

    // --- Variables Internas ---
    private float worldSpeed;
    private float targetSpeed; 
    private float puntajeActual = 0f;
    private float bounceTimer = 0f;
    private bool isBouncing = false;
    private List<GameObject> activeChunks = new List<GameObject>();
    private float spawnZ = 0f;
    private float startTime;
    public bool isGameActive = false;
    
    void Start()
    {
        currentLives = maxLives;
        startTime = Time.time;

        if (BackendManager.instance != null) {
            // Lógica de Revivir
            if (BackendManager.instance.pendingRevive) {
                // NO llamar a StartSession() aquí porque ya tenemos una activa
                
                puntajeActual = PlayerPrefs.GetInt("UltimoPuntaje", 0);
                
                int gemasPrevias = PlayerPrefs.GetInt("UltimasGemas", 0);
                if (ControladorGemas.instancia != null) {
                    ControladorGemas.instancia.RestaurarGemas(gemasPrevias);
                }
                
                BackendManager.instance.pendingRevive = false; // Resetear flag
                
                StartCoroutine(ReviveCountdown());
            } else {
                BackendManager.instance.ResetRunAccumulator(); // Resetear bucket para partida nueva
                BackendManager.instance.StartSession(); // Partida nueva
                StartGame();
            }
        } else {
            StartGame();
        }
    }

    void StartGame() {
        // Iniciamos suaves
        worldSpeed = velocidadBase;
        targetSpeed = velocidadBase; 
        isGameActive = true;
        
        ActualizarVidasUI();

        for (int i = 0; i < chunksOnScreen; i++)
        {
            if (i == 0 && startingChunkPrefab != null) SpawnChunk(startingChunkPrefab);
            else SpawnChunk(); 
        }

        // Inicializar UI de Pausa
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        SetPauseIconState(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private System.Collections.IEnumerator ReviveCountdown() {
        // Inicializar mundo quieto
        isGameActive = false;
        worldSpeed = 0;
        targetSpeed = velocidadBase;
        ActualizarVidasUI();

        // Spawn inicial de trozos
        for (int i = 0; i < chunksOnScreen; i++) {
            if (i == 0 && startingChunkPrefab != null) SpawnChunk(startingChunkPrefab);
            else SpawnChunk();
        }

        // Countdown
        if (countdownText != null) countdownText.gameObject.SetActive(true);
        for (int i = countdownSeconds; i > 0; i--) {
            if (countdownText != null) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        if (countdownText != null) {
            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
            countdownText.gameObject.SetActive(false);
        }

        // Arrancar
        worldSpeed = velocidadBase;
        isGameActive = true;

        // Inicializar UI de Pausa
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        SetPauseIconState(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void HandlePlayerHit(bool escudoActivo = false)
    {
        if (isBouncing) return; 

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.OnPlayerHit();

        // Lógica de Escudo (Script 2)
        if (!escudoActivo)
        {
            currentLives--;
            ActualizarVidasUI();
        }
        else
        {
            Debug.Log("¡El escudo absorbió el golpe! No se pierde vida.");
        }

        if (currentLives <= 0)
        {
            isGameActive = false;
            int finalScore = Mathf.FloorToInt(puntajeActual);
            PlayerPrefs.SetInt("UltimoPuntaje", finalScore);

            int currentGems = (ControladorGemas.instancia != null) ? ControladorGemas.instancia.gemasDeEstaPartida : 0;
            PlayerPrefs.SetInt("UltimasGemas", currentGems);

            int highscore = PlayerPrefs.GetInt("Highscore", 0);
            if (finalScore > highscore) PlayerPrefs.SetInt("Highscore", finalScore);

            // LLAMADA AL BACKEND (ACUMULAR DATOS)
            if (BackendManager.instance != null)
            {
                int currentGemsEarned = (ControladorGemas.instancia != null) ? ControladorGemas.instancia.gemasDeEstaPartida : 0;
                string[] powers = (ControladorPoderes.instancia != null) ? ControladorPoderes.instancia.GetPoderesActivados() : new string[0];
                
                // Guardamos en el "Bucket" del BackendManager
                BackendManager.instance.UpdateRunAccumulator(
                    finalScore, 
                    Time.time - startTime, 
                    currentGemsEarned, 
                    powers
                );

                if (ControladorPoderes.instancia != null) ControladorPoderes.instancia.LimpiarPoderesActivados();
            }

            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            // Activamos el rebote
            bounceTimer = tiempoRebote;
            isBouncing = true;
            
            // Le bajamos un poco la meta de velocidad para que no vuelva de golpe a la velocidad máxima
            targetSpeed = Mathf.Max(velocidadBase, targetSpeed - 5f);
        }
    }

    void ActualizarVidasUI()
    {
        if (vidasUI == null) return;
        for (int i = 0; i < vidasUI.Length; i++)
        {
            if (vidasUI[i] != null) vidasUI[i].SetActive(i < currentLives);
        }
    }

    void Update()
    {
        if (!isGameActive) return;

        // --- MAGIA DE MOVIMIENTO SUAVE ---
        if (isBouncing)
        {
            bounceTimer -= Time.deltaTime;
            // Lerp hace que baje a velocidad negativa de forma curva, no instantánea
            worldSpeed = Mathf.Lerp(worldSpeed, velocidadRebote, Time.deltaTime * 10f);
            if (bounceTimer <= 0f) isBouncing = false;
        }
        else 
        {
            // La meta de velocidad sube muy lento según tu "Aceleración"
            if (targetSpeed < velocidadMaxima) targetSpeed += aceleracion * Time.deltaTime;
            
            // La velocidad del mundo persigue esa meta de forma suave
            worldSpeed = Mathf.Lerp(worldSpeed, targetSpeed, Time.deltaTime * suavidadRecuperacion);
            
            if (worldSpeed > 0)
            {
                // Multiplicador de Score (Script 2)
                int multi = 1;
                if (ControladorPoderes.instancia != null)
                    multi = ControladorPoderes.instancia.MultiplicadorPuntaje;

                puntajeActual += worldSpeed * Time.deltaTime * multi;
                
                if(puntajeTexto != null)
                    puntajeTexto.text = "Score: " + Mathf.FloorToInt(puntajeActual).ToString() + "m";
            }
        }

        // Mover la pista
        float reduccion = (ControladorPoderes.instancia != null) ? ControladorPoderes.instancia.ReduccionVelocidad : 1f;
        float moveDistance = worldSpeed * Time.deltaTime * reduccion;
        foreach (GameObject chunk in activeChunks)
        {
            chunk.transform.Translate(Vector3.back * moveDistance, Space.World);
        }

        // Reciclaje
        float totalChunkSize = chunkLength + chunkGap;
        if (activeChunks.Count > 0 && activeChunks[0].transform.position.z < -totalChunkSize * 1.5f)
        {
            Destroy(activeChunks[0]);
            activeChunks.RemoveAt(0);
            
            float lastChunkZ = activeChunks[activeChunks.Count - 1].transform.position.z;
            SpawnChunk(null, lastChunkZ + totalChunkSize);
        }
    }

    private void SpawnChunk(GameObject prefabToSpawn = null, float zPosition = -999f)
    {
        if (zPosition == -999f) 
        {
            zPosition = spawnZ;
            spawnZ += (chunkLength + chunkGap);
        }

        GameObject prefab = (prefabToSpawn != null) ? prefabToSpawn : chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];
        
        if (prefab != null)
        {
            GameObject newChunk = Instantiate(prefab, new Vector3(0, 0, zPosition), Quaternion.identity);
            newChunk.transform.SetParent(this.transform);
            activeChunks.Add(newChunk);
        }
    }

    // ==========================================
    // SISTEMA DE PAUSA Y COUNTDOWN (Script 1)
    // ==========================================

    public void TogglePause()
    {
        if (isPaused)
        {
            StartCoroutine(ResumeWithCountdown());
        }
        else
        {
            // Pausar
            isPaused = true;
            Time.timeScale = 0f;

            if (pausePanel != null)
                pausePanel.SetActive(true);

            SetPauseIconState(true);
        }
    }

    private void SetPauseIconState(bool paused)
    {
        if (pauseIcon != null) pauseIcon.SetActive(!paused);
        if (playIcon != null) playIcon.SetActive(paused);
    }

    private System.Collections.IEnumerator ResumeWithCountdown()
    {
        // Oculta panel
        if (pausePanel != null)
            pausePanel.SetActive(false);

        SetPauseIconState(false);

        // Mostrar texto
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        for (int i = countdownSeconds; i > 0; i--)
        {
            if (countdownText != null)
                countdownText.text = i.ToString();

            yield return new WaitForSecondsRealtime(1f); // 👈 importante, funciona con Time.timeScale en 0
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
            yield return new WaitForSecondsRealtime(0.5f);
            countdownText.gameObject.SetActive(false);
        }

        // Reanudar juego
        isPaused = false;
        Time.timeScale = 1f;
    }

    // ==========================================
    // CONTROL DE PODERES (MÓVIL)
    // ==========================================

    public void TogglePowersPanel() {
        if (ManejadorPanelPoderes.instancia != null) {
            ManejadorPanelPoderes.instancia.TogglePanel();
        } else {
            Debug.LogWarning("No se encontró una instancia de ManejadorPanelPoderes. Asegúrate de que el script esté en un objeto de la escena.");
        }
    }
}
