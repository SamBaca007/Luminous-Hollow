using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class WarpPortal : MonoBehaviour
{
    [Header("Destino y Vínculo")]
    public Transform exitPoint;
    [Tooltip("Arrastra aquí el objeto del otro portal para vincularlos")]
    public WarpPortal linkedPortal; // --- NUEVO: La conexión con su pareja ---

    [Header("Configuración de Cooldown")]
    public float cooldownDuration = 10f;
    private bool isReady = true;

    [Header("Debug UI")]
    public TextMeshProUGUI debugText;

    [Header("Audio SFX")]
    public AudioClip portalSound;

    private SpriteRenderer spriteRenderer;
    private UIStateManager uiManager;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiManager = FindObjectOfType<UIStateManager>();
        UpdateDebugText(0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isReady)
        {
            // 1. Teletransportamos al jugador primero
            other.transform.position = exitPoint.position;

            // --- NUEVO: Castigo dinámico por usar el portal ---
            if (uiManager != null)
            {
                // Usamos 50 como base, el Manager lo multiplicará según los orbes
                int penalty = uiManager.GetDynamicPunishment(50);
                uiManager.AddScore(-penalty);
                Debug.Log("Portal usado. Penalización dinámica: -" + penalty);
            }

            // 2. Iniciamos el cooldown en ESTE portal
            StartCoroutine(CooldownRoutine());

            // 3. Le avisamos al portal VINCULADO que también inicie su cooldown
            if (linkedPortal != null)
            {
                linkedPortal.TriggerSharedCooldown();
            }

            // --- CAMBIO: Le pedimos al jugador que reproduzca el sonido ---
            if (portalSound != null)
            {
                AudioSource playerAudio = other.GetComponent<AudioSource>();
                if (playerAudio != null)
                {
                    playerAudio.PlayOneShot(portalSound);
                }
            }
        }
    }

    // --- NUEVA FUNCIÓN: Permite que el portal hermano nos apague a distancia ---
    public void TriggerSharedCooldown()
    {
        if (isReady) // Evitamos que se ejecute dos veces por accidente
        {
            StartCoroutine(CooldownRoutine());
        }
    }

    // Separamos el cronómetro de la teletransportación
    private IEnumerator CooldownRoutine()
    {
        isReady = false;
        spriteRenderer.enabled = false;

        float timer = cooldownDuration;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateDebugText(timer);
            yield return null;
        }

        isReady = true;
        spriteRenderer.enabled = true;
        UpdateDebugText(0f);
    }

    private void UpdateDebugText(float timeLeft)
    {
        if (debugText != null)
        {
            if (isReady)
            {
                debugText.text = "Active";
                debugText.color = Color.green;
            }
            else
            {
                debugText.text = "In Cooldown (" + timeLeft.ToString("F1") + ")";
                debugText.color = Color.red;
            }
        }
    }
}