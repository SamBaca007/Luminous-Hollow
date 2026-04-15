using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(PlayerStateManager))]
[RequireComponent(typeof(SpriteRenderer))] // Nos aseguramos de tener acceso a su imagen
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Configuración de Invencibilidad")]
    public float invincibilityDuration = 2f; // Segundos que dura la invencibilidad
    public float blinkInterval = 0.1f;       // Velocidad del parpadeo

    [Header("Interfaz de Usuario")]
    public TextMeshProUGUI healthText;
    public UIStateManager uiManager;

    [Header("Audio")]
    public AudioClip damageSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    private PlayerStateManager stateManager;
    private SpriteRenderer spriteRenderer; // Referencia para hacer el parpadeo

    private bool isDead = false;
    private bool isInvincible = false; // Nuestro "escudo" de i-frames

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        stateManager = GetComponent<PlayerStateManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount)
    {
        // Si ya está muerto o si está en sus frames de invencibilidad, ignoramos el daño por completo
        if (isDead || isInvincible) return;

        currentHealth -= damageAmount;

        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            // --- NUEVO: Sonido al morir ---
            if (deathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deathSound);
            }

            Die();
        }
        else
        {
            // --- NUEVO: Sonido al recibir daño y sobrevivir ---
            if (damageSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(damageSound);
            }

            // Si sobrevivió al golpe, activamos la invencibilidad y el parpadeo
            StartCoroutine(InvincibilityRoutine());
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

        // --- NUEVA LÍNEA: Reproducimos la animación de muerte ---
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

    // --- NUEVO: Rutina de Invencibilidad y Parpadeo ---
    private IEnumerator InvincibilityRoutine()
    {
        // 1. Activamos el escudo protector
        isInvincible = true;

        // --- AVISAMOS QUE EMPIEZA EL ESTADO HURT ---
        stateManager.SetHurtState(true);

        float elapsedTime = 0f;

        // 2. Mientras no se acabe el tiempo de invencibilidad, hacemos el parpadeo
        while (elapsedTime < invincibilityDuration)
        {
            // Alternamos entre visible e invisible
            spriteRenderer.enabled = !spriteRenderer.enabled;

            // Esperamos una fracción de segundo
            yield return new WaitForSeconds(blinkInterval);

            // Sumamos el tiempo que acabamos de esperar al tiempo total transcurrido
            elapsedTime += blinkInterval;
        }

        // 3. Al terminar el tiempo, nos aseguramos de que el sprite esté visible y quitamos el escudo
        spriteRenderer.enabled = true;
        isInvincible = false;

        // --- AVISAMOS QUE TERMINA EL ESTADO HURT ---
        stateManager.SetHurtState(false);
    }
}