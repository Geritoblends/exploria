using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("Arrastra aquí al Auli Boy")]
    public Transform target; 
    
    [Tooltip("Qué tan lejos y alta quieres la cámara")]
    public Vector3 offset = new Vector3(0, 3f, -5f);
    
    [Tooltip("Qué tan suave lo persigue (10 es rápido, 5 es más elástico)")]
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // Calcula a dónde debería ir la cámara
        Vector3 desiredPosition = target.position + offset;
        
        // Bloqueamos la altura (Y) para que la cámara no salte cuando el Auli Boy salta
        desiredPosition.y = offset.y; 

        // Lerp mueve la cámara como si estuviera unida por una liga, no rígidamente
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}