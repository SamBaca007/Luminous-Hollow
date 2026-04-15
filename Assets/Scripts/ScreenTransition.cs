using UnityEngine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenTransition : MonoBehaviour
{
    [Header("Configuración")]
    public string nextSceneName = "Gameplay"; // El nombre de tu escena principal de juego

    void Update()
    {
        // Input.anyKeyDown detecta cualquier pulsación de teclado o clic del ratón
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}