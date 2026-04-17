using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Asegúrate de usar TextMeshPro para el texto
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UIStateManager : MonoBehaviour
{
    // --- CAMBIO 1: Añadimos "Victory" a los posibles estados ---
    public enum GameState { MainMenu, Options, Gameplay, Pause, GameOver, Victory }
    public GameState currentState;

    [Header("UI")]
    // Nota: Como usas dos escenas, deja vacíos los paneles que no existan en la escena actual.
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject gameplayHUD;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI timerText; // El texto en el HUD

    // --- CAMBIO 2: Añadimos el espacio para el panel de victoria ---
    public GameObject victoryPanel;

    [Header("Estado y Textos")]
    public TextMeshProUGUI stateText;

    // --- CAMBIO 3: Añadimos los espacios para los textos de puntuación ---
    public TextMeshProUGUI orbText;   // NUEVO: El texto que dirá "Orbes: X / 30"
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI victoryScoreText;
    public TextMeshProUGUI victoryTimeText;

    // --- CAMBIO 4: Las variables matemáticas para controlar la puntuación ---
    // --- LÓGICA DE ORBES Y PUNTOS ---
    [Header("Configuración de Victoria y Puntuación")]
    public int currentScore = 0;

    public int currentOrbs = 0;       // Contador de orbes
    public int orbsToWin = 30;        // La meta real para ganar
    private float gameTimer = 0f;      // Tiempo total de la partida
    private float survivalTimer = 0f;  // Cronómetro interno para los 15 segundos

    [Header("Escenas")]
    public string menuSceneName = "MainMenu";
    public string gameplaySceneName = "Gameplay";
    public string transitionSceneName = "MiddleScreen";

    [Header("Audio Interfaz")]
    public AudioSource uiAudioSource; // Necesitamos añadirle un AudioSource al objeto que tiene este script
    public AudioClip buttonClickSound;
    public AudioClip pauseSound;
    public AudioClip unpauseSound;

    [Header("Audio BGM y Fin de Juego")]
    public AudioSource bgmSource; // Aquí conectaremos la música de fondo
    public AudioClip victorySound; // El sonido épico de ganar

    [Header("Audio Snapshots")]
    public AudioMixerSnapshot normalSnapshot;
    public AudioMixerSnapshot pausedSnapshot;

    [Header("Audio Snapshots y Mixer")] // (Junto a tus variables de Snapshot)
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Feedback Visual de Puntos")]
    public TextMeshProUGUI scoreFeedbackText; // El texto rojo/verde
    private Coroutine feedbackCoroutine;

    [Header("Rastreador Hitless")]
    public bool isHitlessRun = true; // Empieza en true, se rompe si te pegan
    void Start()
    {
        // Esto asegura que el tiempo corra normal al iniciar cualquier escena
        Time.timeScale = 1f;

        // Detectamos en qué escena estamos para asignar el estado correcto automáticamente
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == gameplaySceneName)
        {
            currentState = GameState.Gameplay;
        }
        else if (currentScene == menuSceneName)
        {
            currentState = GameState.MainMenu;
        }

        // Ahora sí, actualizamos el texto con el estado correcto
        UpdateStateText();

        // --- CAMBIO 5: Inicializamos el texto del Score para que empiece en 0 ---
        UpdateScoreUI();
        UpdateOrbUI(); // --- NUEVO: Inicializamos el texto de orbes en 0/30 ---

        // --- NUEVO: Cargar los volúmenes guardados ---
        // PlayerPrefs.GetFloat("Nombre", ValorPorDefecto)
        float savedMusicVol = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float savedSFXVol = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        // Al cambiar el valor del slider desde código, el slider automáticamente
        // llamará a tus funciones SetMusicVolume y SetSFXVolume por ti.
        if (musicSlider != null) musicSlider.value = savedMusicVol;
        if (sfxSlider != null) sfxSlider.value = savedSFXVol;

        // --- NUEVO: Forzamos al Mixer a aplicar el volumen real desde el milisegundo cero ---
        if (mainMixer != null)
        {
            mainMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Max(0.0001f, savedMusicVol)) * 20);
            mainMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Max(0.0001f, savedSFXVol)) * 20);
        }
    }

    void Update()
    {
        if (currentState == GameState.Gameplay)
        {
            // 1. Aumentamos los relojes
            gameTimer += Time.deltaTime;
            survivalTimer += Time.deltaTime;

            // 2. Actualizamos el texto en pantalla (Formato MM:SS)
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(gameTimer / 60F);
                int seconds = Mathf.FloorToInt(gameTimer - minutes * 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }

            // 3. Bono de Supervivencia (+10 pts cada 15 seg)
            if (survivalTimer >= 15f)
            {
                AddScore(10);
                survivalTimer = 0f; // Reiniciamos el contador
            }
        }
    }

    // --- MÉTODOS PARA EL MENÚ PRINCIPAL ---

    public void PlayGame()
    {
        StartCoroutine(PlayGameRoutine());
    }

    private IEnumerator PlayGameRoutine()
    {
        // 1. Reproducimos el sonido del botón
        PlayButtonSound();

        // 2. Calculamos cuánto dura el sonido (o usamos 0.3 segundos por defecto si no hay sonido)
        float delay = (buttonClickSound != null) ? buttonClickSound.length : 0.3f;

        // 3. Esperamos ese tiempo real (usamos Realtime por si el juego estuviera pausado)
        yield return new WaitForSecondsRealtime(delay);

        // 4. Ahora sí, cambiamos de escena
        Time.timeScale = 1f;
        SceneManager.LoadScene(transitionSceneName);
    }

    public void OpenOptions()
    {
        PlayButtonSound(); // <--- OBLIGAMOS AL SONIDO AQUÍ
        ChangeState(GameState.Options);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        PlayButtonSound(); // <--- OBLIGAMOS AL SONIDO AQUÍ
        ChangeState(GameState.MainMenu);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        StartCoroutine(QuitGameRoutine());
    }

    private IEnumerator QuitGameRoutine()
    {
        // 1. Reproducimos el sonido
        PlayButtonSound();

        // 2. Calculamos el tiempo (0.3 seg por defecto si algo falla)
        float delay = (buttonClickSound != null) ? buttonClickSound.length : 0.3f;

        // 3. Esperamos a que termine el sonido
        yield return new WaitForSecondsRealtime(delay);

        // 4. Ahora sí, cerramos el juego
        Debug.Log("Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- MÉTODOS PARA EL GAMEPLAY ---

    public void PauseGame()
    {
        ChangeState(GameState.Pause);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);

        // Reproducir sonido de pausa
        if (uiAudioSource != null && pauseSound != null) uiAudioSource.PlayOneShot(pauseSound);

        if (pausedSnapshot != null) pausedSnapshot.TransitionTo(0f);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        ChangeState(GameState.Gameplay);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        // Reproducir sonido de quitar pausa
        if (uiAudioSource != null && unpauseSound != null) uiAudioSource.PlayOneShot(unpauseSound);

        if (normalSnapshot != null) normalSnapshot.TransitionTo(0.5f);

        Time.timeScale = 1f;
    }

    public void GoToMainMenu()
    {
        StartCoroutine(GoToMainMenuRoutine());
    }

    private IEnumerator GoToMainMenuRoutine()
    {
        PlayButtonSound(); // Hacemos sonar el clic

        // Calculamos la duración (usamos 0.3s por si acaso)
        float delay = (buttonClickSound != null) ? buttonClickSound.length : 0.3f;

        // Usamos Realtime porque en Game Over o Victoria el tiempo está congelado (Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(delay);

        Time.timeScale = 1f; // Descongelamos antes de viajar

        if (normalSnapshot != null) normalSnapshot.TransitionTo(0f);

        SceneManager.LoadScene(menuSceneName);
    }

    // --- UTILIDADES ---

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        UpdateStateText();
    }

    private void UpdateStateText()
    {
        if (stateText != null)
        {
            stateText.text = "State: " + currentState.ToString();
        }
    }

    // --- LÓGICA DE PUNTUACIÓN Y ORBES ---

    // Añadimos el "bool showFeedback = true" para controlar cuándo sale el texto flotante
    public void AddScore(int points, bool showFeedback = true)
    {
        currentScore += points;
        currentScore = Mathf.Max(0, currentScore);
        UpdateScoreUI();

        // Disparamos la animación del texto de feedback
        if (showFeedback && scoreFeedbackText != null && points != 0)
        {
            if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = StartCoroutine(ShowFeedbackRoutine(points));
        }
    }

    // --- NUEVO: Función exclusiva para recolectar orbes ---
    public void AddOrb()
    {
        currentOrbs++;
        UpdateOrbUI();

        // AHORA los orbes son los que disparan la victoria
        if (currentOrbs >= orbsToWin)
        {
            TriggerVictory();
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    private void UpdateOrbUI()
    {
        if (orbText != null)
        {
            orbText.text = "Orbs: " + currentOrbs + " / " + orbsToWin;
        }
    }

    // --- FUNCIONES DE FIN DE JUEGO ---

    public void TriggerGameOver()
    {
        Debug.Log("PASO 3: UIManager recibió la señal. Cambiando estado a GameOver.");
        ChangeState(GameState.GameOver);

        if (gameplayHUD != null)
        {
            gameplayHUD.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("PASO 4: Panel de Game Over activado con éxito en la jerarquía.");
        }
        else
        {
            Debug.LogError("¡ERROR!: El GameOver Panel no está referenciado en el UIManager.");
        }

        // --- CAMBIO 8: Mostrar la puntuación final al morir ---
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + currentScore;
        }

        if (finalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTimer / 60F);
            int seconds = Mathf.FloorToInt(gameTimer % 60);
            finalTimeText.text = "Time: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // --- NUEVO: Cortar música de fondo ---
        // (El sonido del golpe final y la muerte ya lo reproduce PlayerHealth, así que aquí solo apagamos la música)
        if (bgmSource != null) bgmSource.Stop();

        Time.timeScale = 0f;
    }

    // --- CAMBIO 9: La función que detiene todo y muestra que ganaste ---
    public void TriggerVictory()
    {
        // --- BONOS FINALES DE VICTORIA ---
        if (gameTimer < 210f)
        {
            AddScore(3000, false); // false para que no salga el popup verde
            Debug.Log("¡SUPER SPEEDRUN!");
        }
        else if (gameTimer < 300f)
        {
            AddScore(2000, false);
        }

        // --- NUEVO: Bono Hitless ---
        if (isHitlessRun)
        {
            AddScore(1000, false);
            Debug.Log("¡HITLESS RUN! +1000 Puntos.");
        }

        Debug.Log("¡Meta alcanzada! Cambiando estado a Victory.");

        // --- BONOS DE SPEEDRUN ---
        if (gameTimer < 210f) // 3:30 minutos = 210 segundos
        {
            AddScore(3000);
            Debug.Log("¡SUPER SPEEDRUN!");
        }
        else if (gameTimer < 300f) // 3 minutos = 300 segundos
        {
            AddScore(2000);
            Debug.Log("¡SPEEDRUN NORMAL!");
        }

        Debug.Log("¡Meta alcanzada! Cambiando estado a Victory.");
        ChangeState(GameState.Victory);

        if (gameplayHUD != null)
        {
            gameplayHUD.SetActive(false);
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        // --- NUEVO: Mostrar la puntuación final al ganar ---
        if (victoryScoreText != null)
        {
            victoryScoreText.text = "Final Score: " + currentScore;
        }

        if (victoryTimeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTimer / 60F);
            int seconds = Mathf.FloorToInt(gameTimer % 60);
            victoryTimeText.text = "Time: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // --- NUEVO: Audio de Victoria ---
        if (bgmSource != null) bgmSource.Stop(); // Corta la música
        if (uiAudioSource != null && victorySound != null)
        {
            uiAudioSource.PlayOneShot(victorySound); // Toca el sonido de ganar
        }

        Time.timeScale = 0f;
    }

    public void PlayButtonSound()
    {
        if (uiAudioSource != null && buttonClickSound != null)
        {
            uiAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    // --- CONTROL DE VOLUMEN (SLIDERS) ---

    public void SetMusicVolume(float sliderValue)
    {
        // Guardamos el número en el disco duro del jugador
        PlayerPrefs.SetFloat("MusicVolume", sliderValue); // <-- NUEVO

        float value = Mathf.Max(0.0001f, sliderValue);
        if (mainMixer != null) mainMixer.SetFloat("MusicVol", Mathf.Log10(value) * 20);
    }

    public void SetSFXVolume(float sliderValue)
    {
        // Guardamos el número en el disco duro del jugador
        PlayerPrefs.SetFloat("SFXVolume", sliderValue); // <-- NUEVO

        float value = Mathf.Max(0.0001f, sliderValue);
        if (mainMixer != null) mainMixer.SetFloat("SFXVol", Mathf.Log10(value) * 20);
    }

    // Calcula el castigo dependiendo de tu progreso
    public int GetDynamicPunishment(int basePenalty)
    {
        // Limitamos el multiplicador a un máximo de 2 (Mathf.Min)
        // 0-9 orbes = x1 | 10+ orbes = x2 (Máximo)
        int aggressionMultiplier = Mathf.Min(2, 1 + (currentOrbs / 10));
        return basePenalty * aggressionMultiplier;
    }

    private IEnumerator ShowFeedbackRoutine(int points)
    {
        scoreFeedbackText.gameObject.SetActive(true);

        // Configuramos el texto y el color (+ es verde, - es rojo)
        scoreFeedbackText.text = points > 0 ? "+" + points : points.ToString();
        scoreFeedbackText.color = points > 0 ? Color.green : Color.red;

        // Animación de desvanecimiento (Fade Out)
        float duration = 1.5f;
        float timer = 0f;
        Color startColor = scoreFeedbackText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            scoreFeedbackText.color = Color.Lerp(startColor, endColor, timer / duration);
            yield return null; // Espera al siguiente frame
        }

        scoreFeedbackText.gameObject.SetActive(false);
    }
}