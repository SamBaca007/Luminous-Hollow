# Luminous Hollow

**Juego de Sigilo y Recolección en 2D**

Una experiencia de tensión donde el jugador debe recolectar orbes en una cueva oscura mientras evade a un enemigo implacable impulsado por Inteligencia Artificial.

## Objetivo del Proyecto
Implementar un sistema de juego funcional con arquitectura basada en estados, pathfinding avanzado y audio dinámico e interactivo.

---

## Novedades y Mejoras (Tercer Parcial)

El enfoque de esta etapa del desarrollo fue pulir la experiencia de usuario (UX), añadir profundidad táctica y crear un entorno sonoro inmersivo:

* **IA de Persecución Avanzada:** El enemigo cuenta con navegación inteligente (evasión de obstáculos) y un rango de visión dinámico que castiga los errores del jugador.
* **Sistema de Audio Dinámico:** Implementación de un `AudioMixer` con *Snapshots* para crear transiciones suaves al pausar el juego, y deslizadores de volumen con memoria (`PlayerPrefs`).
* **Interfaz y Debugging:** Pantalla intermedia para contexto narrativo/controles, y un sistema de "Toggle Debug" (F3) oculto para monitorear el estado de las máquinas de estados sin romper la inmersión.
* **Mecánica de Dash:** Una habilidad de evasión rápida para agilizar el movimiento del jugador en momentos críticos.
* **Charcos de Agua (Terreno Dinámico):** Zonas de peligro estático que ralentizan el movimiento del jugador y desactivan temporalmente el uso del Dash.
* **Flash Mágico:** Un recurso defensivo consumible que ciega y paraliza al enemigo durante unos segundos, otorgando una ventana de escape.

---

## Controles

* **WASD / Flechas:** Mover al jugador.
* **[ Espacio ]:** Activar Dash (si no estás sobre agua).
* **[ F ]:** Usar Flash (requiere recargas).
* **[ Esc ]:** Pausar el juego.
* **[ F3 ]:** *Modo Desarrollador* (Muestra/Oculta la UI técnica de las Máquinas de Estados).

---

## Documentación Técnica

Para una revisión profunda de la arquitectura del código y las soluciones implementadas en este parcial, consulta la carpeta `/docs`:

* [Arquitectura del Proyecto](docs/arquitectura.md)
* [Bitácora de Cambios (Parcial 3)](docs/cambios_p3.md)

---

## Detalles Técnicos
* **Motor:** Unity 6 LTS (6000.3.9f1)
* **Lenguaje:** C#
* **Control de Versiones:** Git / GitHub
* **Desarrollador:** Samuel Benjamín Baca Serrano (Universidad Cuauhtémoc, 5A)
