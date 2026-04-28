using UnityEngine;

public class ReviveBridge : MonoBehaviour
{
    public void Revivir()
    {
        if (BackendManager.instance != null)
        {
            Debug.Log("[ReviveBridge] Calling Revive on active instance.");
            BackendManager.instance.PublicRevive();
        }
        else
        {
            Debug.LogError("[ReviveBridge] No BackendManager instance found in the scene!");
        }
    }

    public void Reintentar()
    {
        if (BackendManager.instance != null)
        {
            BackendManager.instance.SendFinalRunRecord();
        }

        ReintentarNivel rn = Object.FindFirstObjectByType<ReintentarNivel>();
        if (rn != null) rn.Reintentar();
    }

    public void IrAlMenu()
    {
        if (BackendManager.instance != null)
        {
            BackendManager.instance.SendFinalRunRecord();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    public void CanjearMonedas(int cantidad)
    {
        if (BackendManager.instance != null)
        {
            BackendManager.instance.PublicExchangeCoinsForGems(cantidad);
        }
    }
}
