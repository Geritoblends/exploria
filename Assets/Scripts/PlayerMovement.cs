using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Referencias")]
    public Animator anim;

    [Header("Colisiones (Modificable en Inspector)")]
    [Tooltip("La Capa (Layer) que usarán los obstáculos")]
    public LayerMask capaObstaculos;
    [Tooltip("Tamaño de la esfera roja")]
    public float radioDeChoque = 0.5f;
    public Vector3 offsetChoque = new Vector3(0, 1f, 0);
    private bool esInmune = false;

    [Header("Carriles")]
    public float laneDistance = 3f;  
    public float laneChangeSpeed = 15f; 

    [Header("Saltos y Gravedad")]
    public float jumpForce = 8f;
    public float gravity = -20f;
    private float pisoY; 
    private bool puedeSaltar = true;

    [Header("Deslizamiento (Slide)")]
    public float slideDuration = 1f;
    private float originalHeight;
    private float originalCenterY;
    private bool isSliding = false;

    private CharacterController controller;
    private Vector3 direction;
    private int currentLane = 1; 

    private Vector2 touchStartPos;
    private bool isSwiping;
    private float minSwipeDistance = 50f; 

    // Bloqueo de Z para que no lo empujen
    private float inicioZ;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        originalHeight = controller.height;
        originalCenterY = controller.center.y;
        
        // Guardamos la posición inicial en Y como nuestro piso
        pisoY = transform.position.y; 
        
        // Guardamos la posición Z inicial para que no lo empujen hacia atrás
        inicioZ = transform.position.z;
    }

    void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.isGameActive)
        {
            // Opcional: Si tienes animaciones, puedes pausarlas o ponerlas en idle aquí
            if (anim != null) anim.speed = 0;
            return;
        }

        if (anim != null) anim.speed = 1;

        // 1. Gravedad Manual
        direction.y += gravity * Time.deltaTime;

        // 2. Leer Controles
        HandleInputs();

        // 3. Calcular posición del carril objetivo
        Vector3 targetPosition = transform.position.z * transform.forward + transform.position.y * transform.up;
        if (currentLane == 0) targetPosition += Vector3.left * laneDistance;
        else if (currentLane == 2) targetPosition += Vector3.right * laneDistance;

        Vector3 diff = targetPosition - transform.position;
        Vector3 moveDir = diff.normalized * laneChangeSpeed * Time.deltaTime;
        
        // 4. Mover al jugador (No hay avance en Z, la pista viene hacia él)
        if (moveDir.sqrMagnitude < diff.sqrMagnitude)
            controller.Move(new Vector3(moveDir.x, direction.y * Time.deltaTime, 0));
        else
            controller.Move(new Vector3(diff.x, direction.y * Time.deltaTime, 0));

        // 5. Tope del piso (Evita que caiga al vacío)
        if (transform.position.y <= pisoY && direction.y < 0)
        {
            transform.position = new Vector3(transform.position.x, pisoY, transform.position.z);
            direction.y = 0f;
        }

        // 6. Blindaje: Forzamos al Auli Boy a no moverse NUNCA hacia atrás o adelante
        Vector3 posFija = transform.position;
        posFija.z = inicioZ;
        transform.position = posFija;

        // 7. Revisar si la esfera choca con algo
        VerificarChoquesEsfera();
    }

    // ==========================================
    // SISTEMA DE CHOQUES (RESOLVED MERGE CONFLICT)
    // ==========================================

    private bool TieneEscudoActivo()
    {
        return ControladorPoderes.instancia != null && ControladorPoderes.instancia.EscudoActivo;
    }

    private void VerificarChoquesEsfera()
    {
        if (esInmune) return;

        Vector3 centroDeLaEsfera = transform.position + offsetChoque;
        Collider[] golpes = Physics.OverlapSphere(centroDeLaEsfera, radioDeChoque, capaObstaculos);

        if (golpes.Length > 0)
        {
            Debug.Log("Choque detectado por la esfera con: " + golpes[0].name);
            AvisarAlGameManager(TieneEscudoActivo());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (esInmune) return;

        if (((1 << other.gameObject.layer) & capaObstaculos) != 0 || other.CompareTag("Obstacle"))
        {
            Debug.Log("Choque detectado por Trigger con: " + other.name);
            AvisarAlGameManager(TieneEscudoActivo());
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (esInmune) return;

        if (((1 << hit.gameObject.layer) & capaObstaculos) != 0 || hit.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Choque detectado físicamente con: " + hit.gameObject.name);
            AvisarAlGameManager(TieneEscudoActivo());
        }
    }

    private void AvisarAlGameManager(bool escudoActivo)
    {
        if (GameManager.instance != null)
        {
            // Make sure your GameManager script has HandlePlayerHit updated to accept a bool!
            GameManager.instance.HandlePlayerHit(escudoActivo); 
        }
    }

    public void OnPlayerHit()
    {
        esInmune = true;
        Invoke(nameof(QuitarInmunidad), 1.5f);
    }

    private void QuitarInmunidad() { esInmune = false; }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + offsetChoque, radioDeChoque);
    }

    // ==========================================
    // CONTROLES Y MOVIMIENTO
    // ==========================================

    private void HandleInputs()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame) MoveLeft();
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame) MoveRight();
            if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame) Jump();
            if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame) Slide();
        }

        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                touchStartPos = touch.position.ReadValue();
                isSwiping = true;
            }
            else if (touch.press.wasReleasedThisFrame && isSwiping)
            {
                Vector2 touchEndPos = touch.position.ReadValue();
                DetectSwipe(touchStartPos, touchEndPos);
                isSwiping = false;
            }
        }
    }

    private void DetectSwipe(Vector2 startPos, Vector2 endPos)
    {
        Vector2 swipeDelta = endPos - startPos;
        if (swipeDelta.magnitude > minSwipeDistance)
        {
            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                if (swipeDelta.x > 0) MoveRight();
                else MoveLeft();
            }
            else
            {
                if (swipeDelta.y > 0) Jump();
                else Slide();
            }
        }
    }

    private void MoveLeft() { if (currentLane > 0) currentLane--; }
    private void MoveRight() { if (currentLane < 2) currentLane++; }

    private void Jump()
    {
        if (puedeSaltar && !isSliding)
        {
            float multiplicador = 1f;

            if (ControladorPoderes.instancia != null && 
                ControladorPoderes.instancia.SuperSaltoActivo)
            {
                multiplicador = 1.6f; // ajusta esto si quieres más salto
            }

            direction.y = jumpForce * multiplicador;
            puedeSaltar = false; 
            
            if (anim != null) anim.SetTrigger("Jump"); 

            Invoke(nameof(RecargarSalto), 1f);
        }
    }

    private void RecargarSalto()
    {
        puedeSaltar = true;
    }

    private void Slide()
    {
        if (!isSliding) StartCoroutine(SlideRoutine());
    }

    private IEnumerator SlideRoutine()
    {
        isSliding = true;

        offsetChoque.y /= 2;
        controller.height = originalHeight / 2;
        controller.center = new Vector3(controller.center.x, originalCenterY / 2, controller.center.z);
        
        if (!puedeSaltar) direction.y = -jumpForce; 

        yield return new WaitForSeconds(slideDuration);

        offsetChoque.y *= 2;
        controller.height = originalHeight;
        controller.center = new Vector3(controller.center.x, originalCenterY, controller.center.z);
        isSliding = false;
    }
}
