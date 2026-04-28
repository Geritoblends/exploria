using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class BackendManager : MonoBehaviour
{
    public static BackendManager instance;

    [Header("Configuración")]
    // public string baseUrl = "https://aulify-api.frontten.dpdns.org"; 
    private string baseUrl = "http://localhost:4000";
    public bool useDevelopmentUrl = true;

    private string sessionToken;
    private int currentSessionId;

    [Header("Datos del Jugador")]
    public int playerId;
    public string playerEmail;
    public int currentGems;
    public int currentCoins;
    public StickerResponse lastSticker;

    public bool pendingRevive = false;
    private bool isReviving = false;

    public event Action OnDataSynced;

    private bool sessionStarted = false;
    private Coroutine heartbeatCoroutine;
[Header("Accumulated Run Data")]
public int currentCaveId = 1;
public int accumulatedScore = 0;
public float accumulatedTime = 0;
public int accumulatedGems = 0;
public HashSet<string> accumulatedPowers = new HashSet<string>();

    private int GetCaveId(string sceneName)
    {
        if (sceneName.Contains("Fuego")) return 1;
        if (sceneName.Contains("Aire")) return 2;
        if (sceneName.Contains("Hielo") || sceneName.Contains("Agua")) return 3; 
        if (sceneName.Contains("Tierra")) return 4;
        
        Debug.LogWarning($"[BackendManager] Scene '{sceneName}' not mapped to a cave. Using fallback ID 1.");
        return 1; // Fallback
    }

private Dictionary<string, int> powerIds = new Dictionary<string, int> {
    { "Iman", 1 },
    { "Magnet", 1 },
    { "SuperJump", 2 },
    { "SuperSalto", 2 },
    { "Super Salto", 2 },
    { "MultiScore", 3 },
    { "ScoreMulti", 3 },
    { "MultiPuntaje", 3 },
    { "MultiGemas", 4 },
    { "GemMulti", 4 },
    { "Escudo", 5 },
    { "Shield", 5 }
};

private Dictionary<string, int> petIds = new Dictionary<string, int> {
    { "Tigrito", 1 },
    { "Pinguino", 2 },
    { "Gato", 3 },
    { "Pajaro", 4 },
    { "Kikiriki", 4 },
    { "Perro", 5 },
    { "Bambi", 6 }
};

void Awake()
{
    Debug.Log("[BackendManager] Awake on object: " + gameObject.name);

    if (instance == null)    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[BackendManager] Instance set successfully.");
        
        // --- TEMPORAL: LIMPIAR COLA ENVENENADA ---
        // Solo lo hacemos una vez al inicio real de la app, no en cada transición de escena
        // PlayerPrefs.DeleteKey("OfflineQueue"); 
        
        InitializeBackend();
    }
    else
    {
        Debug.Log("[BackendManager] Duplicate found, destroying: " + gameObject.name);
        Destroy(gameObject);
        return; 
    }

    isReviving = false; 
}
    private void InitializeBackend()
    {
        sessionToken = PlayerPrefs.GetString("session_token", "");
        if (string.IsNullOrEmpty(sessionToken))
        {
            StartCoroutine(CreatePlayer());
        }
        else
        {
            StartCoroutine(SyncPlayerProfile());
        }
    }

    #region Helpers
    private UnityWebRequest CreateRequest(string path, string method, object body = null)
    {
        string url = baseUrl + path;
        Debug.Log($"[Backend] {method} {url}"); 
        UnityWebRequest request = new UnityWebRequest(url, method);
        request.timeout = 5; 

        if (body != null)
        {
            string json = JsonUtility.ToJson(body);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        }

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(sessionToken))
        {
            request.SetRequestHeader("Authorization", "Bearer " + sessionToken);
        }

        return request;
    }
    #endregion

    #region Scope 1: Auth
    IEnumerator CreatePlayer()
    {
        PlayerCreateRequest body = new PlayerCreateRequest { device_id = SystemInfo.deviceUniqueIdentifier };
        using (UnityWebRequest request = CreateRequest("/api/players", "POST", body))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ApiResponse<PlayerResponse> response = JsonUtility.FromJson<ApiResponse<PlayerResponse>>(request.downloadHandler.text);
                sessionToken = response.data.session_token;
                PlayerPrefs.SetString("session_token", sessionToken);
                PlayerPrefs.Save();
                
                SyncData(response.data);
                Debug.Log("Player created and logged in.");
            }
            else
            {
                Debug.LogWarning("Error creating player (Normal if offline/dev): " + request.error);
                // IMPORTANTE: Permitir que el juego siga aunque no haya servidor
                OnDataSynced?.Invoke();
            }
        }
    }

    IEnumerator SyncPlayerProfile()
    {
        using (UnityWebRequest request = CreateRequest("/api/players/me", "GET"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ApiResponse<PlayerResponse> response = JsonUtility.FromJson<ApiResponse<PlayerResponse>>(request.downloadHandler.text);
                SyncData(response.data);
                Debug.Log("Profile synced.");
            }
            else
            {
                Debug.LogWarning("Error syncing profile (Offline mode): " + request.error);
                if (request.responseCode == 401) 
                {
                    PlayerPrefs.DeleteKey("session_token");
                    StartCoroutine(CreatePlayer());
                }
                else
                {
                    // Disparamos el evento para que la UI se desbloquee
                    OnDataSynced?.Invoke();
                }
            }
        }
    }

    private void SyncData(PlayerResponse data)
    {
        playerId = data.player_id;
        currentGems = data.current_gems;
        currentCoins = data.current_coins;
        playerEmail = data.aulify_email;

        // Start session as soon as we have a valid player, but only ONCE
        StartSession();

        // Process any pending data from previous offline runs
        ProcessOfflineQueue();

        // Auto-fetch sticker if linked
        if (!string.IsNullOrEmpty(playerEmail))
        {
            GetLastSticker((sticker) => {
                lastSticker = sticker;
                OnDataSynced?.Invoke();
            }, (error) => {
                // Even if sticker fetch fails (offline), we must notify UI that sync "finished"
                OnDataSynced?.Invoke();
            });
        }
        else
        {
            OnDataSynced?.Invoke();
        }
    }

    public void ProcessOfflineQueue()
    {
        string queueData = PlayerPrefs.GetString("OfflineQueue", "");
        if (string.IsNullOrEmpty(queueData)) return;

        Debug.Log("[BackendManager] Found pending items in Offline Queue. Processing...");
        StartCoroutine(ProcessQueueRoutine(queueData));
    }

    IEnumerator ProcessQueueRoutine(string queueData)
    {
        string[] lines = queueData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<string> remainingLines = new List<string>();

        foreach (string line in lines)
        {
            string[] parts = line.Split('|');
            if (parts.Length < 2) continue;

            string path = parts[0];
            string json = parts[1];

            Debug.Log($"[Backend Queue] Retrying {path}...");

            using (UnityWebRequest request = CreateRequest(path, "POST", null))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[Backend Queue] Successfully sent pending item: {path}");
                }
                else
                {
                    string errorDetail = request.downloadHandler != null ? request.downloadHandler.text : "No detail";
                    long code = request.responseCode;
                    
                    Debug.LogWarning($"[Backend Queue] Failed to send {path} | Code: {code} | Error: {errorDetail}");

                    // If it's a 4xx error (Bad Request, etc.), it's a logic error. 
                    // Retrying won't help, so we discard it to avoid clogging the queue.
                    if (code >= 400 && code < 500) {
                        Debug.LogWarning($"[Backend Queue] Discarding invalid request (4xx) to {path}");
                    } else {
                        remainingLines.Add(line);
                    }
                }
            }
        }

        string newQueue = remainingLines.Count > 0 ? string.Join("\n", remainingLines) + "\n" : "";
        PlayerPrefs.SetString("OfflineQueue", newQueue);
        PlayerPrefs.Save();
    }

    public void LinkAulify(string email, string password, Action<bool, string> callback = null)
    {
        StartCoroutine(LinkAulifyRoutine(email, password, callback));
    }

    IEnumerator LinkAulifyRoutine(string email, string password, Action<bool, string> callback)
    {
        AulifyLinkRequest body = new AulifyLinkRequest { 
            aulify_email = email,
            aulify_password = password
        };
        using (UnityWebRequest request = CreateRequest("/api/players/me/aulify", "PATCH", body))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Aulify account linked successfully.");
                StartCoroutine(SyncPlayerProfile()); 
                callback?.Invoke(true, "");
            }
            else
            {
                string error = request.downloadHandler != null ? request.downloadHandler.text : request.error;
                Debug.LogWarning("Failed to link Aulify (Offline/Error): " + error);
                callback?.Invoke(false, error);
            }
        }
    }

    public void GetLastSticker(Action<StickerResponse> onSuccess, Action<string> onError)
    {
        Debug.Log("[BackendManager] GetLastSticker requested.");
        StartCoroutine(GetLastStickerRoutine(onSuccess, onError));
    }

    public void TriggerDataSynced()
    {
        OnDataSynced?.Invoke();
    }

    IEnumerator GetLastStickerRoutine(Action<StickerResponse> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest request = CreateRequest("/api/players/me/last-sticker", "GET"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[BackendManager] GetLastSticker Success: " + request.downloadHandler.text);
                try
                {
                    StickerResponse response = JsonUtility.FromJson<StickerResponse>(request.downloadHandler.text);
                    lastSticker = response;
                    
                    // Check for reward
                    if (RewardManager.instance == null) {
                        Debug.LogWarning("[BackendManager] RewardManager.instance is NULL. Searching in scene...");
                        RewardManager.instance = UnityEngine.Object.FindFirstObjectByType<RewardManager>();
                    }

                    if (RewardManager.instance != null)
                    {
                        RewardManager.instance.QueueStickerReward(response.id, response.name, response.sticker_days);
                    }
                    else {
                        Debug.LogWarning("[BackendManager] RewardManager NOT FOUND in scene. Rewards won't work!");
                    }

                    onSuccess?.Invoke(response);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[BackendManager] Error parsing sticker response: " + e.Message);
                    onError?.Invoke("Error al procesar los datos del sticker.");
                }
            }
            else
            {
                string errorMsg = "Error al obtener el último sticker";
                if (request.responseCode == 400 || request.responseCode == 401)
                {
                    ApiResponse<EmptyBody> errorResponse = JsonUtility.FromJson<ApiResponse<EmptyBody>>(request.downloadHandler.text);
                    errorMsg = errorResponse.message;
                }
                
                Debug.LogWarning($"[BackendManager] GetLastSticker failed ({request.responseCode}): {errorMsg}");
                onError?.Invoke(errorMsg);
            }
        }
    }
    #endregion

    #region Scope 2: Sessions
    public void StartSession()
    {
        if (sessionStarted) return; 
        sessionStarted = true;
        StartCoroutine(StartSessionRoutine());
    }

    IEnumerator StartSessionRoutine()
    {
        using (UnityWebRequest request = CreateRequest("/api/sessions", "POST", new EmptyBody()))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                ApiResponse<SessionResponse> response = JsonUtility.FromJson<ApiResponse<SessionResponse>>(request.downloadHandler.text);
                currentSessionId = response.data.audit_session_id;
                
                if (heartbeatCoroutine != null) StopCoroutine(heartbeatCoroutine);
                heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
                
                Debug.Log("AuditSession started: " + currentSessionId);
            }
            else
            {
                sessionStarted = false; // Allow retry if failed
            }
        }
    }

    IEnumerator HeartbeatLoop()
    {
        while (currentSessionId != 0)
        {
            yield return new WaitForSeconds(30f);
            using (UnityWebRequest request = CreateRequest($"/api/sessions/{currentSessionId}/heartbeat", "PATCH", new EmptyBody()))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("Heartbeat failed.");
                }
            }
        }
    }
    #endregion

    #region Scope 3: Runs (Accumulation)

    public void ResetRunAccumulator()
    {
        accumulatedScore = 0;
        accumulatedTime = 0;
        accumulatedGems = 0;
        accumulatedPowers.Clear();
        Debug.Log("[BackendManager] Accumulator reset.");
    }

    public void UpdateRunAccumulator(int score, float time, int gems, string[] powers)
    {
        // Guardamos el ID de la cueva actual (donde el jugador murió)
        currentCaveId = GetCaveId(SceneManager.GetActiveScene().name);

        accumulatedScore = score;
        accumulatedTime += time;
        accumulatedGems += gems;
        if (powers != null)
        {
            foreach (string p in powers) accumulatedPowers.Add(p);
        }
        Debug.Log($"[BackendManager] Updated Bucket: CaveID {currentCaveId}, Score {accumulatedScore}");
    }

    public void SendFinalRunRecord()
    {
        Debug.Log($"[BackendManager] SendFinalRunRecord called. AccumulatedTime: {accumulatedTime}, AccumulatedPowers: {accumulatedPowers.Count}");
        if (accumulatedTime <= 0) return;

        List<ActivationData> activations = new List<ActivationData>();
        foreach(var pName in accumulatedPowers) {
            if (powerIds.TryGetValue(pName, out int pId)) {
                activations.Add(new ActivationData { superpower_id = pId });
            }
            else {
                Debug.LogWarning($"[BackendManager] Power '{pName}' not found in powerIds dictionary.");
            }
        }

        RunResultRequest body = new RunResultRequest
        {
            run = new RunData {
                cave_id = currentCaveId, // Usar el ID guardado, NO el de la escena "Dead"
                score = accumulatedScore,
                gems_earned = accumulatedGems
            },
            activations = activations
        };

        Debug.Log($"[BackendManager] Sending Run: Cave {currentCaveId}, Score {accumulatedScore}, Activations: {activations.Count}");
        StartCoroutine(PostWithQueue("/api/runs", body));
        ResetRunAccumulator();
    }
    #endregion

    #region Scope 4: Economy (Activity Tracking)
    
    public void PurchasePet(string petName, int price, Action<bool> callback = null)
    {
        int pId = 1; // Fallback
        petIds.TryGetValue(petName, out pId);

        PurchasePetRequest purchaseBody = new PurchasePetRequest { pet_id = pId };
        StartCoroutine(PostWithQueue("/api/economy/purchases/pet", purchaseBody, (success, code) => callback?.Invoke(success)));
    }

    public void PurchaseSuperPower(string powerName, int price, Action<bool> callback = null)
    {
        int pId = 1; // Fallback
        powerIds.TryGetValue(powerName, out pId);

        PurchaseSuperPowerRequest purchaseBody = new PurchaseSuperPowerRequest { superpower_id = pId };
        StartCoroutine(PostWithQueue("/api/economy/purchases/superpower", purchaseBody, (success, code) => callback?.Invoke(success)));
    }

    public void SpendCoins(int amount, string reason, Action<bool> callback = null)
    {
        // General spend endpoint
        SpendCoinsRequest body = new SpendCoinsRequest { amount = amount, reason = reason };
        StartCoroutine(PostWithQueue("/api/economy/spend", body, (success, code) => callback?.Invoke(success)));
    }

    public void PublicRevive()
    {
        if (isReviving) return;
        
        Debug.Log("[BackendManager] Triggering Revive via /api/economy/revive (Amount: 50)");
        
        ReviveRequest body = new ReviveRequest { amount = 50 };
        StartCoroutine(PostWithQueue("/api/economy/revive", body, (success, code) => {
            if (success) {
                pendingRevive = true;
                if (MensajesUI.instancia != null) MensajesUI.instancia.Mostrar(MensajesUI.instancia.imgExito);
                
                ReintentarNivel reintentar = UnityEngine.Object.FindFirstObjectByType<ReintentarNivel>();
                if (reintentar != null) reintentar.Reintentar();
            }
        }));
    }

    public void PublicExchangeCoinsForGems(int coinAmount)
    {
        Debug.Log($"[BackendManager] Exchanging {coinAmount} Coins for Gems via /api/economy/exchange (Real-time)");
        ExchangeRequest body = new ExchangeRequest { amount = coinAmount };
        
        StartCoroutine(PostWithQueue("/api/economy/exchange", body, (success, code) => {
            if (success) {
                if (MensajesUI.instancia != null) MensajesUI.instancia.Mostrar(MensajesUI.instancia.imgExito);
            } else {
                if (MensajesUI.instancia != null) {
                    // code 0 = Timeout/Connection Error, 5xx = Server Error
                    if (code == 0 || (code >= 500 && code < 600)) {
                        MensajesUI.instancia.Mostrar(MensajesUI.instancia.imgAviso); // "No pudimos realizar la operacion"
                    } else {
                        MensajesUI.instancia.Mostrar(MensajesUI.instancia.imgError); // "No tienes aulicoins suficientes"
                    }
                }
            }
        }, true));
    }

    #endregion

    #region Offline Queue System
    
    IEnumerator PostWithQueue(string path, object body, Action<bool, long> callback = null, bool forceRealtime = false)
    {
        string json = JsonUtility.ToJson(body);
        Debug.Log($"[Backend Request] {path} | Body: {json}");
        
        using (UnityWebRequest request = CreateRequest(path, "POST", body))
        {
            yield return request.SendWebRequest();

            long code = request.responseCode;

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler != null ? request.downloadHandler.text : "No response body";
                Debug.Log($"[Backend Success] {path} | Response: {responseText}");
                
                callback?.Invoke(true, code);
            }
            else
            {
                string errorDetail = request.downloadHandler != null ? request.downloadHandler.text : "No detail";
                Debug.LogWarning($"[Backend Error] {path} | Code: {code} | Error: {request.error} | Detail: {errorDetail}");
                
                if (forceRealtime) {
                    callback?.Invoke(false, code);
                } else {
                    SaveToOfflineQueue(path, json);
                    callback?.Invoke(true, code); // Still return success to UI for "Instant Feel"
                }
            }
        }
    }

    private void SaveToOfflineQueue(string path, string json)
    {
        string queue = PlayerPrefs.GetString("OfflineQueue", "");
        queue += $"{path}|{json}\n";
        PlayerPrefs.SetString("OfflineQueue", queue);
        PlayerPrefs.Save();
    }

    #endregion

    #region DTOs
    [Serializable] public class EmptyBody { }
    [Serializable] public class ReviveRequest { public int amount; }
    [Serializable] public class PlayerCreateRequest { public string device_id; }
    [Serializable] public class AulifyLinkRequest { 
        public string aulify_email; 
        public string aulify_password;
    }

    // MATCHING SCHEMAS
    [Serializable] public class PurchasePetRequest { public int pet_id; }
    [Serializable] public class PurchaseSuperPowerRequest { public int superpower_id; }
    [Serializable] public class SpendCoinsRequest { public int amount; public string reason; }
    [Serializable] public class SpendGemsRequest { public int amount; }
    [Serializable] public class ExchangeRequest { public int amount; }

    [Serializable]
    public class RunResultRequest
    {
        public RunData run;
        public List<ActivationData> activations;
    }

    [Serializable]
    public class RunData {
        public int cave_id;
        public int score;
        public int gems_earned;
    }

    [Serializable]
    public class ActivationData {
        public int superpower_id;
    }

    [Serializable]
    public class ApiResponse<T>
    {
        public bool success;
        public string message;
        public T data;
    }

    [Serializable]
    public class PlayerResponse
    {
        public int player_id;
        public string session_token;
        public int current_gems;
        public int current_coins;
        public string aulify_email;
    }

    [Serializable]
    public class SessionResponse
    {
        public int audit_session_id;
    }

    [Serializable]
    public class SessionHeartbeatRequest {
        public int audit_session_id;
    }

    [Serializable]
    public class StickerResponse
    {
        public int id;
        public string name;
        public string description;
        public int sticker_days;
        public string image; // Absolute URL to the PNG image
    }
    #endregion
}
