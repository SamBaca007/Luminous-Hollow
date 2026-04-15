using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OrbController : MonoBehaviour
{
    [Header("Configuración del Orbe")]
    public int pointsValue = 100;
    public Transform[] spawnPoints; // Lista de lugares seguros

    [Header("Audio SFX")]
    public AudioClip orbPickupSound;

    private UIStateManager uiManager;

    void Start()
    {
        // Buscamos el UIManager automáticamente al iniciar
        uiManager = FindObjectOfType<UIStateManager>();

        // Nos movemos a un punto al azar desde el principio
        MoveToRandomSpawnPoint();
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