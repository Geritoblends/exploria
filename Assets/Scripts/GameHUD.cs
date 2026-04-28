using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameHUD - UI de juego estilo Subway Surfers
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("=== BOTÓN PAUSA ===")]
    public Button pauseButton;
    public GameObject pauseIcon;
    public GameObject playIcon;

    [Header("=== VIDAS ===")]
    public TMP_Text livesText;
    public Image heartIcon;
    public int maxLives = 5;

    [Header("=== PROGRESO / MULTIPLICADOR ===")]
    public TMP_Text multiplierText;
    public Image starIcon;

    [Header("=== PUNTUACIÓN ===")]
    public TMP_Text scoreText;

    [Header("=== GEMAS RECOLECTADAS ===")]
    public TMP_Text gemsText;
    public Image gemIcon;

    [Header("=== PANEL DE PAUSA ===")]
    public GameObject pausePanel;

    [Header("=== CUENTA REGRESIVA ===")]
    [Tooltip("Texto grande centrado que muestra 3, 2, 1 al reanudar (desactivado al inicio)")]
    public TMP_Text countdownText;
    [Tooltip("Segundos de cuenta regresiva al reanudar")]
    public int countdownSeconds = 3;

    // ── Estado interno ──────────────────────────────────────────
    private bool _isPaused = false;
    private int  _currentLives;
    private int  _score;
    private int  _multiplier = 1;
    private int  _gems = 0;

    // ── Propiedades públicas ────────────────────────────────────
    public bool IsPaused     => _isPaused;
    public int  CurrentLives => _currentLives;
    public int  Score        => _score;
    public int  Multiplier   => _multiplier;
    public int  Gems         => _gems;

    // ────────────────────────────────────────────────────────────
    void Start()
    {
        _currentLives = maxLives;

        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        SetPauseIconState(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        RefreshHUD();
    }

    // ────────────────────────────────────────────────────────────
    //  PAUSA / REANUDAR
    // ────────────────────────────────────────────────────────────

    public void TogglePause()
    {
        if (_isPaused)
        {
            // Reanudar — lanza cuenta regresiva
            StartCoroutine(ResumeWithCountdown());
        }
        else
        {
            // Pausar — inmediato
            _isPaused = true;
            Time.timeScale = 0f;
            SetPauseIconState(true);
            if (pausePanel != null) pausePanel.SetActive(true);
        }
    }

    private IEnumerator ResumeWithCountdown()
    {
        // Oculta panel de pausa y cambia ícono
        if (pausePanel != null) pausePanel.SetActive(false);
        SetPauseIconState(false);

        // Muestra cuenta regresiva (juego sigue pausado con timeScale = 0)
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        for (int i = countdownSeconds; i > 0; i--)
        {
            if (countdownText != null)
                countdownText.text = i.ToString();

            // WaitForSecondsRealtime no se ve afectado por timeScale = 0
            yield return new WaitForSecondsRealtime(1f);
        }

        // Oculta el texto y reanuda
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        _isPaused = false;
        Time.timeScale = 1f;
    }

    public void Pause()
    {
        if (!_isPaused) TogglePause();
    }

    public void Resume()
    {
        if (_isPaused) TogglePause();
    }

    private void SetPauseIconState(bool paused)
    {
        if (pauseIcon != null) pauseIcon.SetActive(!paused);
        if (playIcon  != null) playIcon.SetActive(paused);
    }

    // ────────────────────────────────────────────────────────────
    //  VIDAS
    // ────────────────────────────────────────────────────────────

    public bool LoseLife()
    {
        _currentLives = Mathf.Max(0, _currentLives - 1);
        RefreshHUD();
        return _currentLives > 0;
    }

    public void GainLife()
    {
        _currentLives = Mathf.Min(maxLives, _currentLives + 1);
        RefreshHUD();
    }

    public void ResetLives()
    {
        _currentLives = maxLives;
        RefreshHUD();
    }

    // ────────────────────────────────────────────────────────────
    //  PUNTUACIÓN
    // ────────────────────────────────────────────────────────────

    public void AddScore(int basePoints)
    {
        _score += basePoints * _multiplier;
        RefreshHUD();
    }

    public void ResetScore()
    {
        _score = 0;
        RefreshHUD();
    }

    // ────────────────────────────────────────────────────────────
    //  GEMAS
    // ────────────────────────────────────────────────────────────

    public void AddGems(int amount)
    {
        _gems += Mathf.Max(0, amount);
        RefreshHUD();
    }

    public void ResetGems()
    {
        _gems = 0;
        RefreshHUD();
    }

    // ── NUEVO: CONTROL DE PODERES ────────────────────────────────
    
    public void TogglePowersPanel() {
        ManejadorPanelPoderes manejador = Object.FindFirstObjectByType<ManejadorPanelPoderes>();
        if (manejador != null) {
            manejador.TogglePanel();
        } else {
            Debug.LogWarning("No se encontró un ManejadorPanelPoderes en la escena.");
        }
    }

    // ────────────────────────────────────────────────────────────
    //  MULTIPLICADOR
    // ────────────────────────────────────────────────────────────

    public void SetMultiplier(int value)
    {
        _multiplier = Mathf.Max(1, value);
        RefreshHUD();
    }

    // ────────────────────────────────────────────────────────────
    //  ACTUALIZAR VISUAL
    // ────────────────────────────────────────────────────────────

    public void RefreshHUD()
    {
        if (livesText      != null) livesText.text      = $"{_currentLives}/{maxLives}";
        if (scoreText      != null) scoreText.text      = _score.ToString("D6");
        if (multiplierText != null) multiplierText.text = $"x{_multiplier}";
        if (gemsText       != null) gemsText.text       = _gems.ToString();
    }

    // ────────────────────────────────────────────────────────────
    //  USO DESDE OTROS SCRIPTS
    // ────────────────────────────────────────────────────────────
    /*
    GameHUD hud;

    void Start() { hud = FindObjectOfType<GameHUD>(); }

    void OnGemCollected()             { hud.AddGems(1); }
    void OnCoinCollected()            { hud.AddScore(10); }
    void OnPlayerHit()                { bool alive = hud.LoseLife(); if (!alive) GameOver(); }
    void OnMultiplierZoneEnter(int n) { hud.SetMultiplier(n); }
    void OnNewGame()                  { hud.ResetScore(); hud.ResetLives(); hud.ResetGems(); }
    */
}