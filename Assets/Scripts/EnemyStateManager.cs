using UnityEngine;
using UnityEngine.AI; // --- NUEVO: Necesario para usar el NavMesh ---
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NavMeshAgent))] // --- NUEVO ---
public class EnemyStateManager : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack, Stunned }
    public EnemyState currentState = EnemyState.Patrol;

    [Header("Interfaz de Usuario")]
    public TextMeshProUGUI enemyStateText;

    [Header("Referencias")]
    public Transform player;
    private Rigidbody2D rb;
    private NavMeshAgent agent; // --- NUEVO: El cerebro de la IA ---

    [Header("Configuración de Patrullaje")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    public float patrolSpeed = 2f;

    [Header("Configuración de Persecución y Ataque")]
    public float chaseSpeed = 3.5f;
    public float detectionRadius = 4f;
    public float attackRadius = 1f;
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;

    [Header("Configuración de Recompensas")]
    public float nearMissThreshold = 1.5f; // Qué tan cerca debes estar para que cuente el esquive
    public float nearMissCooldown = 4f;    // Segundos antes de poder hacer otro Near Miss
    private float lastNearMissTime = -10f; // Empezamos en negativo para que el primer esquive siempre funcione

    [Header("Escalado de Dificultad Dinámica")]
    public float maxChaseSpeed = 7f;      // Velocidad al tener 30 orbes (Casi tan rápido como tú)
    public float maxDetectionRadius = 8f; // Rango de visión gigante al final

    private float baseChaseSpeed;
    private float baseDetectionRadius;

    public event System.Action OnAttackEvent;
    public Vector2 MovementDirection { get; private set; }
    private UIStateManager uiManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>();
        uiManager = FindObjectOfType<UIStateManager>();

        // Guardamos las estadísticas base
        baseChaseSpeed = chaseSpeed;
        baseDetectionRadius = detectionRadius;

        // --- NUEVO: Configuración CRÍTICA para que el NavMesh funcione en 2D ---
        agent.updateRotation = false; // Evita que el enemigo rote en 3D
        agent.updateUpAxis = false;   // Mantiene el eje Z plano

        UpdateStateText();
    }

    void Update()
    {
        if (player == null) return;

        // --- LA NUEVA EVOLUCIÓN DEL ENEMIGO ---
        if (uiManager != null)
        {
            // 1. Calculamos el progreso crudo (0.0 a 1.0)
            float rawProgress = (float)uiManager.currentOrbs / (float)uiManager.orbsToWin;

            // 2. LA MAGIA: Elevamos el progreso a la potencia de 2.5
            // Esto crea una curva lenta al principio que sube de golpe al final.
            float curveProgress = Mathf.Pow(rawProgress, 2.5f);

            // Ajustamos las estadísticas usando la nueva curva
            chaseSpeed = Mathf.Lerp(baseChaseSpeed, maxChaseSpeed, curveProgress);
            detectionRadius = Mathf.Lerp(baseDetectionRadius, maxDetectionRadius, curveProgress);
        }

        // Extraemos la dirección en la que el NavMeshAgent se está moviendo
        MovementDirection = agent.velocity.normalized;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        EnemyState previousState = currentState;

        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior();
                if (distanceToPlayer <= detectionRadius)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                ChaseBehavior(distanceToPlayer);
                break;

            case EnemyState.Attack:
                AttackBehavior(distanceToPlayer);
                break;
        }

        if (currentState != previousState)
        {
            UpdateStateText();
        }
    }

    // ELIMINAMOS EL FIXED UPDATE. El NavMeshAgent mueve al personaje automáticamente de forma fluida.

    private void PatrolBehavior()
    {
        if (waypoints.Length == 0) return;

        agent.isStopped = false; // Nos aseguramos de que pueda moverse
        agent.speed = patrolSpeed; // Velocidad de patrulla

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // --- NUEVO: Le damos las coordenadas al GPS de la IA ---
        agent.SetDestination(targetWaypoint.position);

        // Checamos si ya llegó a su destino
        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.5f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private void ChaseBehavior(float distanceToPlayer)
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;

        // --- NUEVO: Persigue al jugador usando el mejor camino posible ---
        agent.SetDestination(player.position);

        if (distanceToPlayer <= attackRadius)
        {
            currentState = EnemyState.Attack;
        }
        else if (distanceToPlayer > detectionRadius)
        {
            currentState = EnemyState.Patrol;
        }
    }

    private void AttackBehavior(float distanceToPlayer)
    {
        agent.isStopped = true; // --- NUEVO: Clavamos los frenos para atacar ---

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log("¡El enemigo ha iniciado la animación de ataque!");
            OnAttackEvent?.Invoke();

            lastAttackTime = Time.time;
        }

        if (distanceToPlayer > attackRadius)
        {
            currentState = EnemyState.Chase;
        }
    }

    private void UpdateStateText()
    {
        if (enemyStateText != null)
        {
            enemyStateText.text = "Enemy State: " + currentState.ToString();
        }
    }

    public void DealDamage()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // 1. GOLPE CONECTADO
            if (distanceToPlayer <= attackRadius + 0.5f)
            {
                Collider2D myCollider = GetComponent<Collider2D>();
                player.GetComponent<PlayerHealth>()?.TakeDamage(1, myCollider);

                Debug.Log("¡Golpe conectado! HP reducido.");
            }
            // 2. NEAR MISS (Esquive Perfecto)
            // Solo cuenta si estás FUERA del ataque, pero DENTRO de la zona de riesgo
            else if (distanceToPlayer <= attackRadius + 0.5f + nearMissThreshold)
            {
                // Comprobamos si el Cooldown ya terminó
                if (Time.time >= lastNearMissTime + nearMissCooldown)
                {
                    if (uiManager != null)
                    {
                        uiManager.AddScore(30);
                        Debug.Log("¡Near Miss! +30 puntos.");
                    }
                    // Reiniciamos el reloj para que no lo puedan farmear
                    lastNearMissTime = Time.time;
                }
                else
                {
                    Debug.Log("Esquive exitoso, pero el Near Miss está en Cooldown.");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Lo que ya tenías (Círculos de visión)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // 2. Lo nuevo (Los Rayos X del NavMesh)
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.magenta;
            var path = agent.path;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                // Dibuja una línea magenta mostrando la ruta exacta
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            }
        }
    }

    public void StunEnemy()
    {
        if (currentState != EnemyState.Stunned)
        {
            // --- NUEVO: Recompensa por usar el Flash ---
            if (uiManager != null)
            {
                uiManager.AddScore(150);
                Debug.Log("¡Enemigo cegado! +150 puntos.");
            }

            StartCoroutine(StunRoutine());
        }
    }

    private System.Collections.IEnumerator StunRoutine()
    {
        EnemyState previousState = currentState;
        currentState = EnemyState.Stunned;

        UpdateStateText();

        agent.isStopped = true; // --- NUEVO: Frenamos la IA ---

        GetComponent<SpriteRenderer>().color = Color.blue;

        yield return new WaitForSeconds(3f);

        GetComponent<SpriteRenderer>().color = Color.white;

        currentState = previousState;
        UpdateStateText();
    }

}