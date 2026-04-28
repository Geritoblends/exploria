using UnityEngine;

public class LuzFuego : MonoBehaviour
{
    private Light luz;
    public float intensidadMin = 1.5f;
    public float intensidadMax = 2.5f;

    void Start() { luz = GetComponent<Light>(); }

    void Update()
    {
        // Esto cambia la intensidad al azar para simular el parpadeo del fuego
        luz.intensity = Random.Range(intensidadMin, intensidadMax);
    }
}