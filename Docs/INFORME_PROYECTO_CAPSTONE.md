# PROYECTO CAPSTONE: DESARROLLO DE UN VIDEOJUEGO 3D INTERACTIVO
## Informe Técnico y Documento de Diseño de Juego (GDD)

---

### DATOS GENERALES
* **Título del Videojuego:** Crystal Relic: Temple Escape
* **Motor de Desarrollo:** Unity 6 (versión 6000.6.5f1)
* **Lenguaje:** C# (.NET Standard / Unity API)
* **Género:** Plataformas 3D, Aventura y Escape
* **Estructura:** 3 Niveles Progresivos (MainScene, Level_2, Level_3)
* **Tiempo estimado de gameplay:** 3 a 7 minutos
* **Plataforma objetivo:** PC (Windows Standalone x86_64)
* **Integrantes del Equipo:** [Nombre y Apellidos de los Integrantes]

---

## 1. DESCRIPCIÓN DEL VIDEOJUEGO Y CONCEPTO GENERAL
*Crystal Relic: Temple Escape* es una experiencia tridimensional de plataformas y exploración en tercera persona ambientada en las ruinas flotantes de un templo místico. El jugador controla a un niño explorador que debe navegar entre plataformas suspendidas sobre el abismo, sortear trampas de púas letales y recolectar reliquias de energía indispensables para reactivar el portal de escape ancestral a lo largo de 3 niveles progresivos.

La atmósfera combina elementos arquitectónicos de piedra y tecnología de cristal brillante, complementados por efectos visuales de partículas, un sistema dinámico de iluminación y una banda sonora ambiental envolvente.

---

## 2. OBJETIVO Y GAME LOOP (CICLO DE JUEGO)

### 2.1. Objetivo Principal
El jugador debe explorar el nivel y recolectar **5 Cristales de Energía** dispersos en diferentes secciones del escenario. Al obtener la totalidad de los cristales, se activa el **Portal de Salida**. Cruzar dicho portal con vida consagra la victoria.

### 2.2. Condiciones de Victoria y Derrota
* **Condición de Victoria:** Recolectar los 5 cristales y entrar en contacto con el Portal de Salida activado. El sistema registra el tiempo total transcurrido y despliega la pantalla de felicitaciones.
* **Condición de Derrota (Game Over):** El jugador dispone de una reserva de **3 Vidas** y un indicador de Salud. Si cae al vacío o sufre daño por trampas y enemigos sin vidas restantes, se detiene la partida y se muestra el menú de derrota con opción de reintento.

```
+------------------+      +--------------------+      +---------------------+
|  Menú / Inicio   | ---> |  Gameplay Activo   | ---> | Recolección 5/5     |
+------------------+      +--------------------+      +---------------------+
                                    |                            |
                                    v                            v
                          [Derrota: 0 Vidas]            [Victoria: Portal]
```

---

## 3. MECÁNICAS DE JUEGO E INTERACCIÓN
1. **Desplazamiento y Salto en Tercera Persona:** Sistema de movimiento libre con traslación basada en la orientación relativa de la cámara, cálculo de gravedad cuadrática y salto responsivo mediante el *New Input System* de Unity.
2. **Cámara Orbital Libre:** Sistema de cámara en tercera persona que orbita al avatar con suavizado posicional y control del cursor bloqueado para máxima precisión.
3. **Mecánica de Recolección (Collectibles):** Los cristales poseen animación continua de rotación e inclinación sinusoidal. Al colisionar, emiten partículas brillantes, reproducen sonido de feedback y actualizan el contador global.
4. **Puntos de Control (Checkpoints):** Estructuras interactivas que cambian de color a verde al ser cruzadas, registrando la posición segura de reaparición del jugador.
5. **Obstáculos y Trampas:** Esferas de púas móviles y estáticas que infligen daño al jugador y activan cuadros de invulnerabilidad temporal (*i-frames*).

---

## 4. SISTEMAS IMPLEMENTADOS Y ARQUITECTURA DE CÓDIGO
El proyecto aplica principios de arquitectura limpia con separación de responsabilidades:

| Script / Sistema | Responsabilidad Principal | Patrón de Diseño |
| :--- | :--- | :--- |
| **`GameManager`** | Administra estados globales (`Playing`, `Victory`, `GameOver`, `Paused`), objetivos y flujo de escenas. | Singleton |
| **`PlayerHealth`** | Gestiona vidas, salud, daño por trampas/caídas y parpadeo de invulnerabilidad. | Componente Modular |
| **`EnemyAI`** | Inteligencia Artificial del enemigo con comportamientos de patrulla y persecución. | Finite State Machine (FSM) |
| **`CollectibleItem`**| Lógica de recogida, animación procedural e invocación de efectos de partículas. | Observer / Eventos |
| **`ExitPortal`** | Escucha el evento de cristales completos, activa efectos visuales y define la victoria. | Observer |
| **`UIManager`** | Actualiza el HUD en tiempo real (vidas, cristales, objetivos) y despliega pantallas modales. | Singleton / UI Handler |
| **`AudioManager`** | Reproductor centralizado de música de fondo y efectos de sonido con respaldo procedural. | Singleton |
| **`ObjectPool`** | Administrador de reciclaje de GameObjects para optimización de memoria. | Object Pool Pattern |

---

## 5. INTELIGENCIA ARTIFICIAL (IA BÁSICA)
Se implementó un **Enemigo Guardián** que utiliza la tecnología nativa de navegación de Unity (`NavMeshAgent`) gobernado por una Máquina de Estados Finitos (FSM):

1. **Estado Patrulla (Patrol):**
   * El enemigo se desplaza a velocidad moderada (2.8 m/s) entre una serie de *Waypoints* predefinidos.
   * Al alcanzar cada punto, realiza una breve pausa antes de dirigirse al siguiente objetivo de la ruta.
2. **Detección por Distancia y Visión:**
   * Evalúa la proximidad del jugador en un radio de 8 metros.
   * Ejecuta un `Physics.Linecast` (Raycast) para comprobar si existe línea de visión despejada o si hay obstáculos que bloquean la visibilidad.
3. **Estado Persecución (Chase):**
   * Al detectar al avatar, incrementa su velocidad a 5.0 m/s y persigue activamente al jugador.
   * Si el jugador logra distanciarse más de 12 metros, el enemigo abandona la cacería y regresa a su ruta de patrulla.
4. **Estado Ataque:**
   * Al alcanzar el radio cuerpo a cuerpo (1.6 metros), aplica daño a la salud del jugador respetando un tiempo de enfriamiento (*cooldown*).

---

## 6. TÉCNICAS DE OPTIMIZACIÓN APLICADAS

Para dar riguroso cumplimiento a la consigna académica, el proyecto incorporó **tres técnicas de optimización**:

### 6.1. Object Pooling (Reutilización de Objetos)
En lugar de invocar costosas operaciones de `Instantiate()` y `Destroy()` cada vez que se emiten partículas de recolección de cristales o efectos de impacto, se implementó la clase `ObjectPool.cs`. Esta reserva en memoria un conjunto de instancias inactivas en el inicio (`Awake`) y las activa/desactiva bajo demanda, eliminando las pausas por recolección de basura (*Garbage Collection Spikes*).

### 6.2. Reducción de Cálculos Innecesarios en IA (Pathfinding Throttling)
La llamada a `agent.SetDestination(player.position)` en cada frame dentro de `Update()` genera una saturación crítica de la CPU al recalcular caminos 60 veces por segundo. Se implementó una técnica de *Throttling* que evalúa la trayectoria del NavMesh cada **0.2 segundos** mediante un temporizador (`pathUpdateInterval`), reduciendo la carga de cómputo del algoritmo de búsqueda de rutas en más de un 85%.

### 6.3. Level of Detail (LOD Groups)
Los modelos 3D del entorno y los coleccionables (`LODGroup`) reducen dinámicamente el número de polígonos dibujados según la distancia de la cámara, optimizando la tasa de fotogramas por segundo (FPS).

---

## 7. INTERFAZ DE USUARIO (UI) Y AMBIENTACIÓN AUDIOVISUAL

* **HUD en Pantalla:** Muestra en tiempo real la cantidad de cristales recolectados (`0 / 5`), el número de vidas disponibles y el texto orientativo con la misión activa.
* **Paneles Modales:** Menú de Pausa accesible con la tecla `ESC`, Pantalla de Victoria con cronómetro de tiempo empleado y Pantalla de Game Over con botones de reinicio inmediato.
* **Audio:** Música temática en bucle y efectos sonoros espaciales y 2D (salto, recolección de cristal, activación de checkpoint, impacto y fanfarria de victoria).
* **Partículas y Luces:** Sistema de partículas doradas al recoger ítems y halo luminoso de color cian indicando la apertura del portal final.

---

## 8. PRUEBAS REALIZADAS Y RESULTADOS
* **Pruebas de Movimiento y Cámara:** Se verificó la estabilidad del `CharacterController` al saltar sobre bordes y rampas sin atascos.
* **Pruebas de Detección de Colisiones:** Los triggers de caída al abismo (`fallDeathThreshold`) y trampas de púas reaccionan instantáneamente sin pérdida de cuadros de contacto.
* **Pruebas de IA en NavMesh:** Se validó que el enemigo no atraviese geometrías estáticas y responda con precisión al rango de visión.
* **Pruebas de Compilación (Standalone Build):** Se generó y ejecutó el archivo ejecutable `.exe` fuera de Unity, comprobando un rendimiento estable a 60 FPS en resolución 1080p.

---

## 9. CONCLUSIONES
El proyecto integra de manera armónica los pilares de la programación de videojuegos 3D en Unity: controladores de personaje, física y colliders, inteligencia artificial basada en agentes de navegación, arquitectura modular desacoplada mediante eventos, feedback audiovisual e interfaces de usuario reactivas. Se priorizó la estabilidad, la finalización de un ciclo de juego completo y la aplicación de técnicas formales de optimización de software.
