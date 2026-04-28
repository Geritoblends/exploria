using UnityEngine;

public class MantenimientoPantalla : MonoBehaviour
{
    void Awake()
    {
        // Esto evita que la pantalla se apague mientras la app esté activa
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
