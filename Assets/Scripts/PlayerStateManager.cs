using UnityEngine;
using TMPro;
using System.Collections; // --- NUEVO: Necesario para usar las Corrutinas (IEnumerator) ---

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStateManager : MonoBehaviour
{
    public enum PlayerState { Idle, Moving, Hurt, Dead, Dashing }
    public PlayerState currentState = PlayerState.Idle;

    [Header("Interfaz de Usuario")]
    public TextMeshProUGUI playerStateText;

    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public bool canMove = true;

    // --- Variables del Dash ---
    [Header("Configuración del Dash")]
    public float dashSpeedMultiplier = 3f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 1f;

    private bool canDash = true;
    public bool isDashing = false;
    // ---------------------------------

    [Header("Inventario y UI")]
    public int flashbangCount = 0;
    public TextMeshProUGUI flashbangText;

    [Header("Audio")]
    public AudioClip flashSound;
    private AudioSource audioSource;

    [Header("Audio Extra")]
    public AudioSource footstepSource;

    [Header("Audio SFX")]
    public AudioClip dashSound;

    // --- Variables para el agua ---
    private float baseMoveSpeed;
    public bool isInPuddle = false;
    public Vector2 AnimDirection { get; private set; }

    private Rigidbody2D rb;
    private Vector2 movement;

    private enum AxisPriority { None, Horizontal, Vertical }
    private AxisPriority currentPriority = AxisPriority.None;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>(); // --- Obtiene el audio ---
        baseMoveSpeed = moveSpeed; // --- Guardamos la velocidad normal ---
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        UpdateStateText();
        UpdateFlashbangText(); // --- Actualizamos el texto a 0 al iniciar ---
    }

    void Update()
    {
        if (!canMove || currentState == PlayerState.Dead)
        {
            movement = Vector2.zero;
            AnimDirection = Vector2.zero;
            UpdateStateText();
            return;
        }

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        movement = new Vector2(inputX, inputY).normalized;

        // --- LÓGICA DE PASOS ---
        // Si el jugador se está moviendo, no está muerto, no está dasheando
        if (movement != Vector2.zero && currentState != PlayerState.Dead && !isDashing)
        {
            // Verificamos si NO está sonando ya, para no reiniciarlo en cada frame
            if (footstepSource != null && !footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else
        {
            // Si nos detenemos, detenemos el audio de inmediato
            if (footstepSource != null && footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
        // -------------------------

        // --- Detectar el input del Dash ---
        // Verificamos que presione Espacio, que el cooldown esté listo, que no esté ya haciendo dash, y que no esté lastimado
        if (Input.GetKeyDown(KeyCode.Space) && canDash && !isDashing && currentState != PlayerState.Hurt && !isInPuddle)
        {
            if (movement != Vector2.zero)
            {
                StartCoroutine(DashRoutine());
            }
        }
        // ----------------------------------------

        // Justo debajo de donde pusiste el input del Dash
        if (Input.GetKeyDown(KeyCode.F) && flashbangCount > 0 && currentState != PlayerState.Dead)
        {
            UseFlashbang();
        }

        // Lógica de Prioridad para la animación
        if (inputX == 0 && inputY == 0)
        {
            currentPriority = AxisPriority.None;
            AnimDirection = Vector2.zero;

            // --- SOLO cambiamos a Idle si no estamos lastimados NI haciendo dash ---
            if (currentState != PlayerState.Hurt && currentState != PlayerState.Dashing)
            {
                currentState = PlayerState.Idle;
            }
        }
        else
        {
            // --- SOLO cambiamos a Moving si no estamos lastimados NI haciendo dash ---
            if (currentState != PlayerState.Hurt && currentState != PlayerState.Dashing)
            {
                currentState = PlayerState.Moving;
            }

            if (currentPriority == AxisPriority.None)
            {
                if (inputX != 0) currentPriority = AxisPriority.Horizontal;
                else if (inputY != 0) currentPriority = AxisPriority.Vertical;
            }
            else if (currentPriority == AxisPriority.Horizontal && inputX == 0 && inputY != 0)
            {
                currentPriority = AxisPriority.Vertical;
            }
            else if (currentPriority == AxisPriority.Vertical && inputY == 0 && inputX != 0)
            {
                currentPriority = AxisPriority.Horizontal;
            }

            if (currentPriority == AxisPriority.Horizontal)
            {
                AnimDirection = new Vector2(inputX, 0);
            }
            else if (currentPriority == AxisPriority.Vertical)
            {
                AnimDirection = new Vector2(0, inputY);
            }
        }

        UpdateStateText();
    }

    void FixedUpdate()
    {
        // --- Aplicamos la velocidad del Dash si está activo ---
        if (isDashing)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * dashSpeedMultiplier * Time.fixedDeltaTime);
        }
        else
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void UpdateStateText()
    {
        if (playerStateText != null)
        {
            playerStateText.text = "Player State: " + currentState.ToString();
        }
    }

    public void Die()
    {
        currentState = PlayerState.Dead;
        canMove = false;
        UpdateStateText();
    }

    public void SetHurtState(bool isHurt)
    {
        if (currentState == PlayerState.Dead) return;

        if (isHurt)
        {
            currentState = PlayerState.Hurt;
        }
        else
        {
            if (movement == Vector2.zero) currentState = PlayerState.Idle;
            else currentState = PlayerState.Moving;
        }

        UpdateStateText();
    }

    // --- La Corrutina que controla el tiempo del Dash ---
    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        // --- NUEVO: Sonar el Dash ---
        if (dashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dashSound);
        }

        currentState = PlayerState.Dashing;
        UpdateStateText();

        // 1. Esperamos el tiempo que dura el impulso
        yield return new WaitForSeconds(dashDuration);

        // 2. Terminó el impulso, regresamos el control
        isDashing = false;

        // Verificamos qué estaba haciendo al terminar el dash para regresarlo a ese estado
        if (currentState == PlayerState.Dashing)
        {
            if (movement == Vector2.zero) currentState = PlayerState.Idle;
            else currentState = PlayerState.Moving;
        }

        UpdateStateText();

        // 3. Esperamos el tiempo de recarga antes de permitir otro Dash
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // --- Función que llamarán los charcos ---
    public void SetPuddleEffect(bool inPuddle)
    {
        isInPuddle = inPuddle;
        if (isInPuddle)
        {
            moveSpeed = baseMoveSpeed / 2f; // Cortamos la velocidad a la mitad
        }
        else
        {
            moveSpeed = baseMoveSpeed; // Restauramos la velocidad
        }
    }

    public void AddFlashbang()
    {
        flashbangCount++;
        UpdateFlashbangText(); // --- Contador de flashes ---
    }

    private void UseFlashbang()
    {
        flashbangCount--;
        UpdateFlashbangText(); // --- Contador de flashes ---

        // --- Reproducir el sonido ---
        if (flashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(flashSound);
        }

        EnemyStateManager enemy = Object.FindFirstObjectByType<EnemyStateManager>();
        if (enemy != null)
        {
            enemy.StunEnemy();
        }
    }

    // --- NUEVO ---
    private void UpdateFlashbangText()
    {
        if (flashbangText != null)
        {
            flashbangText.text = "Flashes on Inventory: " + flashbangCount.ToString();
        }
    }

}
