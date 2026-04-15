using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo a Seguir")]
    public Transform target; // Aquí arrastraremos al Jugador

    [Header("Configuración de la Cámara")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f; // Qué tan suave será el seguimiento
    public Vector3 offset = new Vector3(0f, 0f, -10f); // El -10 en Z es vital en 2D

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        // Si por alguna razón el jugador no está asignado o es destruido, no hacemos nada
        if (target == null)
        {
            return;
        }

        // Calculamos la posición a la que la cámara debería ir
        Vector3 desiredPosition = target.position + offset;

        // SmoothDamp crea una transición fluida entre la posición actual y la deseada
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

        // Aplicamos la nueva posición a la cámara
        transform.position = smoothedPosition;
    }
}