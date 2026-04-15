Arquitectura Técnica - Luminous Hollow  
Este documento detalla la estructura lógica y la comunicación entre los sistemas principales de Luminous Hollow. El proyecto sigue un patrón de diseño centrado en un Gestor de Estados Centralizado y una jerarquía de audio ruteada mediante Audio Mixers.  
  
1. Gestión de Estados (UIStateManager)  
El núcleo del juego es el UIStateManager.cs, que actúa como el "cerebro" de la sesión. Utiliza una máquina de estados finitos sencilla basada en un enum:  
  
Estados: MainMenu, Options, Gameplay, Pause, GameOver, Victory.  
  
Responsabilidades:  
  
Activación/Desactivación de paneles de la UI.  
  
Control del Time.timeScale (congelar el juego en pausa o fin de partida).  
  
Gestión de la puntuación actual y verificación de la condición de victoria.  
  
Transiciones de audio mediante Snapshots.  
  
2. Sistema de Audio y Mezcla  
Se implementó una arquitectura de audio robusta para permitir el control independiente del usuario y efectos ambientales automáticos.  
  
Jerarquía del Audio Mixer  
Para evitar conflictos entre los Sliders de volumen (control del jugador) y los Snapshots de pausa (control del juego), se diseñó una jerarquía de "Padre-Hijo":  
  
Master: Canal final.  
  
Musica (Exposed): Controlado por el Slider del menú.  
  
MusicEffects: Canal hijo donde actúan los Snapshots. Esto permite bajar el volumen al pausar sin "bloquear" el valor del slider principal.  
  
SFX (Exposed): Controlado por el Slider para todos los efectos (pasos, daño, orbes).  
  
Snapshots  
Normal: Volumen de Musica_Efectos a 0dB.  
  
Pausado: Volumen de Musica_Efectos a -15dB (efecto de atenuación).  
  
3. Lógica de Gameplay e Interacción  
El juego utiliza una arquitectura desacoplada donde los objetos del mundo interactúan con el jugador y el gestor de estados:  
  
Orbes y Portales: Utilizan OnTriggerEnter2D. Para asegurar que los sonidos no se corten al destruir o mover el objeto, estos envían una señal al AudioSource del Jugador para reproducir el clip.  
  
Sistema de Daño (PlayerHealth): Gestiona la integridad del jugador, activando corrutinas de invencibilidad y comunicando la muerte al UIStateManager.  
  
Navegación del Enemigo: Basada en un sistema de IA que evalúa el rango de visión dinámico y utiliza un mapa de navegación para evitar obstáculos y zonas no caminables.  
  
4. Persistencia de Datos  
Se utiliza la clase PlayerPrefs para asegurar que las preferencias del usuario trasciendan la sesión:  
  
Claves guardadas: MusicVolume, SFXVolume.  
  
Sincronización: Al iniciar el Start(), el UIStateManager recupera estos valores, ajusta los Sliders visuales y actualiza los parámetros expuestos del Mixer inmediatamente para evitar ruidos fuertes al arrancar.  
  
5. Herramientas de Desarrollo (Debug System)  
Se implementó un sistema de Toggle Debug accesible mediante la tecla F3. Este sistema activa un contenedor de UI etiquetado como DebugUI que muestra en tiempo real:  
  
El estado actual de la IA.  
  
El estado lógico del UIStateManager.  
  
Contadores técnicos.  
Este sistema está desacoplado de la lógica principal para ser activado o desactivado sin afectar el rendimiento de la build final.
