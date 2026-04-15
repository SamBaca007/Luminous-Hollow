using UnityEngine;

public class DebugToggle : MonoBehaviour
{
    [Header("Contenedor Principal")]
    [Tooltip("Arrastra aquí el objeto vacío que contiene todos los textos de debug")]
    public GameObject debugContainer;

    void Start()
    {
        // Opcional: Apagar los textos automáticamente al iniciar el juego para la versión final
        if (debugContainer != null)
        {
            debugContainer.SetActive(false);
        }
    }

    void Update()
    {
        // Detecta si se presiona la tecla F3
        if (Input.GetKeyDown(KeyCode.F3))
        {
            if (debugContainer != null)
            {
                // activeSelf devuelve 'true' si está prendido y 'false' si está apagado.
                // El signo '!' invierte ese valor (lo prende si está apagado y viceversa).
                debugContainer.SetActive(!debugContainer.activeSelf);
            }
        }
    }
}