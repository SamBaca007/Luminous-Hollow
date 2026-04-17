using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OrbController : MonoBehaviour
{
    [Header("Configuración del Orbe")]
    public int pointsValue = 100;
    public Transform[] spawnPoints; // Lista de lugares seguros

    [Header("Audio SFX")]
    public AudioClip orbPickupSound;

    [Header("Radar Audiovisual")]
    public float maxHearingDistance = 15f; // A qué distancia se empieza a escuchar/ver
    public SpriteRenderer glowSprite;
    private AudioSource humAudio;
    private SpriteRenderer orbSprite;
    private Transform playerTransform;

    private UIStateManager uiManager;
    private Vector3 baseScale;

    void Start()
    {
        // Buscamos el UIManager automáticamente al iniciar
        uiManager = FindObjectOfType<UIStateManager>();

        // Nos movemos a un punto al azar desde el principio
        MoveToRandomSpawnPoint();

        humAudio = GetComponent<AudioSource>();
        orbSprite = GetComponent<SpriteRenderer>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Guardamos el tamaño que le pusiste en el editor de Unity
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (playerTransform != null)
        {
            // Calculamos a qué distancia está el jugador
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            // 1. AUDIO: Es más fuerte cuando la distancia es cercana a 0
            float audioIntensity = 1f - Mathf.Clamp01(distance / maxHearingDistance);
            if (humAudio != null) humAudio.volume = audioIntensity;

            // 2. VISUAL (Saviavida): El brillo de lejos y se apaga de cerca
            if (glowSprite != null)
            {
                // visualIntensity es 1 cuando estás lejos (15m) y 0 cuando estás encima (0m)
                float visualIntensity = Mathf.Clamp01(distance / maxHearingDistance);

                Color glowColor = glowSprite.color;
                // El canal Alpha (transparencia) se ata directamente a la distancia
                glowColor.a = visualIntensity;
                glowSprite.color = glowColor;
            }
        }
    }

    // Esta función se activa cuando alguien atraviesa el orbe
    void OnTriggerEnter2D(Collider2D other)
    {
        // Revisamos si quien lo tocó fue el Jugador
        if (other.CompareTag("Player"))
        {
            // --- CAMBIO: Le pedimos al jugador que reproduzca el sonido ---
            if (orbPickupSound != null)
            {
                AudioSource playerAudio = other.GetComponent<AudioSource>();
                if (playerAudio != null)
                {
                    playerAudio.PlayOneShot(orbPickupSound);
                }
            }

            if (uiManager != null)
            {
                uiManager.AddScore(pointsValue);
                uiManager.AddOrb();              // --- NUEVO: Avisamos que recogió un orbe ---
            }

            int dropChance = Random.Range(0, 4); // Genera un número: 0, 1, 2 o 3
            if (dropChance == 0) // Si sale 0 (1 de 4 probabilidades)
            {
                // Le damos el flash al jugador
                other.GetComponent<PlayerStateManager>().AddFlashbang();
                Debug.Log("¡Flash Mágico conseguido!");
            }

            MoveToRandomSpawnPoint();
        }
    }

    private void MoveToRandomSpawnPoint()
    {
        if (spawnPoints.Length > 0)
        {
            // Elegimos un número al azar entre 0 y la cantidad de puntos que hayas puesto
            int randomIndex = Random.Range(0, spawnPoints.Length);

            // Movemos el orbe a esa posición
            transform.position = spawnPoints[randomIndex].position;
        }
    }
}