using UnityEngine;

public class ItemGema : MonoBehaviour {

    private Transform jugador;
    public float velocidadIman = 12f;
    public float rangoIman = 10f;

    void Start() {
        GameObject obj = GameObject.FindWithTag("Player");
        if (obj != null) jugador = obj.transform;
    }

    void Update() {
        if (ControladorPoderes.instancia == null) return;
        if (!ControladorPoderes.instancia.ImanActivo) return;
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        // Solo se activa si está dentro del rango
        if (distancia <= rangoIman) {
            transform.position = Vector3.MoveTowards(
                transform.position,
                jugador.position,
                velocidadIman * Time.deltaTime
            );
        }
    }

    void OnTriggerEnter(Collider otro) {
        // Dentro del OnTriggerEnter cuando el niño toca la gema:
        if (AudioManager.instancia != null) {
         AudioManager.instancia.PlayGema();
        }
        // ... aquí sigue tu código de sumar gema y el Destroy(gameObject)
        if (otro.CompareTag("Player")) {
            ControladorGemas.instancia.SumarGema();
            Destroy(gameObject);
        }
    }
}