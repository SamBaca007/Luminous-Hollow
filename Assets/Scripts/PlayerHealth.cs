using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(PlayerStateManager))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))] // --- NUEVO: Necesitamos el collider del jugador
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Configuración de Invencibilidad")]
    public float invincibilityDuration = 2f;
    public float blinkInterval = 0.1f;

    [Header("Interfaz de Usuario")]
    public TextMeshProUGUI healthText;
    public UIStateManager uiManager;

    [Header("Audio")]
    public AudioClip damageSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    private PlayerStateManager stateManager;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider; // --- NUEVO: Referencia a nuestro collider

    private bool isDead = false;
    private bool isInvincible = false;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        stateManager = GetComponent<PlayerStateManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>(); // --- NUEVO: Lo asignamos al inicio
        UpdateHealthUI();
    }

    // --- CAMBIO: Añadimos 'Collider2D enemyCollider' como parámetro opcional ---
    public void TakeDamage(int damageAmount, Collider2D enemyCollider = null)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damageAmount;

        if (uiManager != null)
        {
            // --- NUEVO: Adiós al sueño del Hitless ---
            uiManager.isHitlessRun = false;

            int penalty = uiManager.GetDynamicPunishment(200);
            uiManager.AddScore(-penalty); // Como no pusimos "false", aparecerá el texto ROJO flotando

            Debug.Log("¡Golpe! Puntos perdidos: -" + penalty);
        }

        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            if (deathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deathSound);
            }

            Die();
        }
        else
        {
            if (damageSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(damageSound);
            }

            // Le pasamos el collider del enemigo a la corrutina
            StartCoroutine(InvincibilityRoutine(enemyCollider));
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth + " / " + maxHealth;
        }
    }

    private void Die()
    {
        isDead = true;
        spriteRenderer.enabled = true;
        stateManager.Die();
        GetComponent<PlayerAnimator>()?.TriggerDeathAnimation();
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (uiManager != null)
        {
            uiManager.TriggerGameOver();
        }
    }

    // --- CAMBIO: La corrutina ahora recibe el collider del enemigo ---
    private IEnumerator InvincibilityRoutine(Collider2D enemyCollider)
    {
        isInvincible = true;
        stateManager.SetHurtState(true);

        // 1. Apagamos la colisión física con ESTE enemigo en específico
        if (enemyCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, enemyCollider, true);
        }

        float elapsedTime = 0f;

        while (elapsedTime < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        // 2. Restauramos todo a la normalidad
        spriteRenderer.enabled = true;
        isInvincible = false;
        stateManager.SetHurtState(false);

        // 3. Volvemos a encender la colisión con el enemigo
        if (enemyCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, enemyCollider, false);
        }
    }
}