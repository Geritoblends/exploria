using UnityEngine;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    public static RewardManager instance;

    private Queue<string> rewardQueue = new Queue<string>();
    private bool isShowingReward = false;

    void Awake()
    {
        Debug.Log("[RewardManager] Awake running on " + gameObject.name);
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[RewardManager] Instance set successfully.");
        }
        else
        {
            Debug.LogWarning("[RewardManager] Duplicate found, destroying!");
            Destroy(gameObject);
        }
    }

    public void QueueStickerReward(int stickerId, string stickerName, int days)
    {
        int lastSavedId = PlayerPrefs.GetInt("LastStickerId", -1);
        int lastSavedDays = PlayerPrefs.GetInt("LastStickerDays", -1);
        
        // Reward if it's a completely new ID OR if the day count has increased
        if (stickerId != lastSavedId || days > lastSavedDays)
        {
            Debug.Log($"[RewardManager] Reward Condition Met! ID: {stickerId} (Old: {lastSavedId}), Days: {days} (Old: {lastSavedDays}). Queuing reward.");
            
            PlayerPrefs.SetInt("LastStickerId", stickerId);
            PlayerPrefs.SetInt("LastStickerDays", days);
            PlayerPrefs.Save();

            rewardQueue.Enqueue(stickerName);
            Debug.Log($"[RewardManager] Reward added to queue for: {stickerName}. Total in queue: {rewardQueue.Count}");
            
            TryShowNextReward();
        }
        else
        {
            Debug.Log($"[RewardManager] No reward needed. ID {stickerId} and Days {days} already rewarded.");
        }
    }

    public void TryShowNextReward()
    {
        // Only show rewards in MainMenu (you can add more scenes if needed)
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName != "MainMenu" && sceneName != "Main") return; 

        if (rewardQueue.Count > 0 && !isShowingReward)
        {
            string stickerName = rewardQueue.Dequeue();
            ApplyReward(stickerName);
        }
    }

    private void ApplyReward(string stickerName)
    {
        StartCoroutine(ApplyRewardRoutine(stickerName));
    }

    private System.Collections.IEnumerator ApplyRewardRoutine(string stickerName)
    {
        isShowingReward = true;

        // 1. Give Gems
        int currentGems = PlayerPrefs.GetInt("GemasTotales", 0);
        PlayerPrefs.SetInt("GemasTotales", currentGems + 100);
        PlayerPrefs.Save();
        
        Debug.Log($"[RewardManager] Gems added (+100). Current: {PlayerPrefs.GetInt("GemasTotales")}");

        // 2. Update currency displays immediately
        if (BackendManager.instance != null) BackendManager.instance.TriggerDataSynced();

        // 3. Wait for UI to be ready (in case we are transitioning scenes)
        float timeout = 3f;
        while (MensajesUI.instancia == null && timeout > 0)
        {
            Debug.Log("[RewardManager] Waiting for MensajesUI.instancia...");
            yield return new WaitForSeconds(0.2f);
            timeout -= 0.2f;
        }

        // 4. Show UI Feedback
        if (MensajesUI.instancia != null)
        {
            Debug.Log($"[RewardManager] Showing popup for: {stickerName}");
            MensajesUI.instancia.Mostrar(MensajesUI.instancia.imgExito);
        }
        else
        {
            Debug.LogWarning("[RewardManager] MensajesUI.instancia not found after waiting. Skipping popup.");
        }

        // Allow next reward after a delay
        yield return new WaitForSeconds(2.5f);
        isShowingReward = false;
        TryShowNextReward();
    }
}
