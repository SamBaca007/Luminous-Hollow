using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("UI del Mapa")]
    public GameObject mapPanel; // El panel que oscurece la pantalla
    public RectTransform mapBackground; // Tu imagen del mapa dibujado

    [Header("Iconos en la UI")]
    public RectTransform playerIcon;
    public RectTransform enemyIcon;
    public RectTransform orbIcon;

    [Header("Objetos en el Mundo Real")]
    public Transform playerTransform;
    public Transform enemyTransform;
    public Transform orbTransform; // Como tu orbe solo se teletransporta, podemos referenciarlo directo

    [Header("Calibración del Mundo Real")]
    [Tooltip("Coordenadas de la esquina inferior izquierda de tu nivel transitable")]
    public Vector2 worldBottomLeft;
    [Tooltip("Coordenadas de la esquina superior derecha de tu nivel transitable")]
    public Vector2 worldTopRight;

    void Start()
    {
        // El mapa empieza apagado
        if (mapPanel != null) mapPanel.SetActive(false);
    }

    void Update()
    {
        // Activar/Desactivar con la letra M
        if (Input.GetKeyDown(KeyCode.M))
        {
            mapPanel.SetActive(!mapPanel.activeSelf);
        }

        // Solo hacemos los cálculos matemáticos si el jugador está mirando el mapa
        if (mapPanel.activeSelf)
        {
            UpdateIconPosition(playerTransform, playerIcon);
            UpdateIconPosition(enemyTransform, enemyIcon);
            UpdateIconPosition(orbTransform, orbIcon);
        }
    }

    private void UpdateIconPosition(Transform worldObject, RectTransform icon)
    {
        if (worldObject == null || icon == null) return;

        // 1. Obtener la posición real en el juego
        Vector2 realPos = worldObject.position;

        // 2. Calcular el porcentaje (0.0 a 1.0) de dónde está en relación a las esquinas del mundo
        float normalizedX = Mathf.InverseLerp(worldBottomLeft.x, worldTopRight.x, realPos.x);
        float normalizedY = Mathf.InverseLerp(worldBottomLeft.y, worldTopRight.y, realPos.y);

        // 3. Convertir ese porcentaje al tamaño de la imagen del mapa en la interfaz
        float mapWidth = mapBackground.rect.width;
        float mapHeight = mapBackground.rect.height;

        // Como el ancla de la imagen está en el centro, ajustamos la posición
        float iconX = (normalizedX * mapWidth) - (mapWidth / 2f);
        float iconY = (normalizedY * mapHeight) - (mapHeight / 2f);

        // 4. Mover el icono
        icon.anchoredPosition = new Vector2(iconX, iconY);
    }
}