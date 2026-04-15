using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PuddleTrap : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // Si el jugador pisa el charco
        if (other.CompareTag("Player"))
        {
            PlayerStateManager player = other.GetComponent<PlayerStateManager>();
            if (player != null)
            {
                player.SetPuddleEffect(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Si el jugador sale del charco
        if (other.CompareTag("Player"))
        {
            PlayerStateManager player = other.GetComponent<PlayerStateManager>();
            if (player != null)
            {
                player.SetPuddleEffect(false);
            }
        }
    }
}