Bitácora de Cambios - Tercer Parcial  
  
Este documento detalla las implementaciones, mejoras y correcciones realizadas en Luminous Hollow durante el tercer parcial. El enfoque principal estuvo en la mejora de la Inteligencia Artificial, la retroalimentación auditiva y la pulcritud de la interfaz para la compilación final (Build).  
  
1. Nuevas Mecánicas de Juego implementadas  
Inteligencia Artificial Mejorada: Se reestructuró la navegación del enemigo. Ahora cuenta con un rango de visión dinámico (cono de visión ampliado) y un sistema de pathfinding que detecta y evade obstáculos físicos y zonas marcadas como no transitables.  
  
Sistema de Dash: Se añadió un impulso rápido para el jugador, añadiendo una capa de agilidad para evadir persecuciones inminentes.  
  
Terreno Dinámico (Charcos de Agua): Se crearon zonas que afectan directamente la física del jugador, ralentizando su velocidad de movimiento general y deshabilitando la mecánica de Dash mientras se encuentre sobre ellas.  
  
Consumible Defensivo (Flash): Se integró un ataque de aturdimiento temporal. Al usarlo, el enemigo entra en un estado de parálisis por unos segundos, ofreciendo una ventana de escape estratégica.  
  
2. Reestructuración del Sistema de Audio  
El audio pasó de ser estático a ser un sistema dinámico completamente ruteado a través de un Audio Mixer central (MainMixer).  
  
Control de Usuario Persistente: Se implementaron deslizadores (Sliders) en el menú de opciones para Música y SFX. Los valores se exponen al código y se guardan localmente utilizando PlayerPrefs, asegurando que la configuración del jugador sobreviva entre sesiones.  
  
Audio Ducking (Atenuación): Se configuraron AudioMixer Snapshots (Normal y Pausado). Al pausar el juego, el volumen de la música baja suavemente a -15dB, y regresa con un fade-in de 0.5 segundos al reanudar.  
  
Centralización de Eventos: Los sonidos de recolección (Orbes) y teletransporte (Portales) se redirigieron para ser reproducidos a través del canal SFX del jugador, evitando que el sonido se corte al destruir el objeto recolectable.  
  
3. Interfaz de Usuario (UI) y Debugging  
Pantalla de Contexto: Se añadió una escena/pantalla intermedia antes del Gameplay para establecer el contexto narrativo y mostrar los controles, guiando al jugador sin necesidad de un tutorial externo.  
  
Modo Desarrollador (Toggle Debug): Se agrupó toda la información en pantalla sobre las máquinas de estado (IA, Jugador, Sistema) en un contenedor etiquetado como DebugUI. Mediante un script global, esta información se oculta al iniciar el juego y puede ser alternada utilizando la tecla F3.  
  
Tipografía: Se integró una fuente de letra personalizada y coherente en todos los textos de la interfaz utilizando TextMeshPro.  
  
4. Retos Técnicos y Soluciones Aplicadas  
Durante el proceso de desarrollo e integración, se presentaron diversos desafíos técnicos que requirieron refactorización:  
  
Conflicto de Volúmenes en Pausa: El Snapshot de pausa intentaba modificar el mismo parámetro de volumen que el Slider bloqueaba por código.  
Se dividió el canal de música en una jerarquía Padre-Hijo (Musica -> Musica_Efectos). El Slider controla al padre y el Snapshot interactúa con el hijo.  
  
Reinicio de la BGM en el Menú: Al abrir el menú de opciones, el panel principal se desactivaba, destruyendo el reproductor de audio asociado a él.  
Se reubicó el objeto Audio_Musica_Menu en la raíz de la escena (fuera del Canvas) y se utilizó CloseOptions() en lugar de recargar la escena.  
  
Pérdida de Escala en la Build Final: Unity intentaba forzar proporciones distintas a las diseñadas (Free Aspect), deformando la UI.  
Se configuró el Canvas Scaler a Scale With Screen Size (1920x1080), se ancló el Aspect Ratio a 16:9 y se habilitó VSync en los Player Settings para evitar tearing.  
