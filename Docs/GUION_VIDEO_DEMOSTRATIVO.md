# GUION TÉCNICO PARA EL VIDEO DEMOSTRATIVO (5 A 7 MINUTOS)
## Proyecto Capstone: Videojuego 3D Interactivo - Unity & C#

---

### ESTRUCTURA GENERAL DEL VIDEO

| Minuto | Sección | Contenido a Mostrar en Pantalla | Quién habla / Qué decir |
| :--- | :--- | :--- | :--- |
| **0:00 - 1:00** | **1. Introducción y Presentación del Proyecto** | Pantalla de inicio / Vista panorámica del escenario 3D. | Presentación del equipo, título del juego (*Crystal Relic: Temple Escape*), género y objetivo del proyecto. |
| **1:00 - 2:30** | **2. Gameplay: Movimiento, Salto y Recolección** | Jugador desplazándose entre plataformas, saltando y recogiendo cristales. | Explicación del `playerController` con New Input System, cámara orbital y recolección de los 5 cristales. |
| **2:30 - 3:45** | **3. Inteligencia Artificial (Patrulla y Persecución)** | Enfocar al enemigo guardián patrullando en sus waypoints y luego persiguiendo al jugador. | Explicación de la FSM de `EnemyAI`, uso de `NavMeshAgent` y detección por Raycast. |
| **3:45 - 4:45** | **4. UI, Audio, VFX y Trampas** | HUD en pantalla (cristales, vidas), sonido de pasos/recolección y activación del checkpoint verde. | Detallar la arquitectura de `UIManager`, `AudioManager` y el sistema de partículas al tomar cristales. |
| **4:45 - 6:00** | **5. Técnica de Optimización y Arquitectura de Código** | Mostrar en Visual Studio / Unity el código de `ObjectPool.cs` y el temporizador en `EnemyAI.cs`. | **Punto Clave de Evaluación:** Explicar el Object Pooling para evitar Garbage Collection y la reducción de llamadas de pathfinding. |
| **6:00 - 7:00** | **6. Condición de Victoria, Build Standalone y Cierre** | Jugador cruzando el portal activado -> Pantalla de Victoria con tiempo. Demostrar el juego corriendo en su ejecutable `.exe`. | Conclusiones finales y agradecimiento. |

---

### GUION NARRADO PASO A PASO (TEXTO GUÍA PARA LEER O PARAFRASEAR)

#### Minuto 0:00 - 1:00 | Introducción y Pantalla de Inicio
> *"Buenos días, profesor y compañeros. En este video les presentamos nuestro proyecto Capstone: 'Crystal Relic: Temple Escape', desarrollado en Unity 6 y C#. 
> Al iniciar el juego, nos recibe una interfaz de inicio con la ventana principal y el botón 'PLAY' para dar comienzo a la partida. 
> Nuestro juego presenta una experiencia 3D de plataformas y escape a través de 3 niveles progresivos con un ciclo de juego completo: inicio, exploración, amenaza con IA y condiciones de victoria y derrota."*

#### Minuto 1:00 - 2:30 | Gameplay y Mecánica Principal
> *"Como pueden observar en pantalla, el jugador controla a un niño explorador con un sistema de movimiento en tercera persona que utiliza el New Input System de Unity y un CharacterController. La cámara orbital en tercera persona rota suavemente con el mouse.
> La mecánica central consiste en recolectar 5 Cristales de Energía para activar el portal de escape antiguo. Cada cristal cuenta con animación procedural de rotación y flotación, además de generar feedback sonoro y un efecto de partículas al ser obtenido."*

#### Minuto 2:30 - 3:45 | Inteligencia Artificial (IA)
> *"Para cumplir con el requerimiento de IA con al menos dos comportamientos, implementamos un enemigo Guardián controlado por NavMesh y una Máquina de Estados Finitos.
> En su estado por defecto, el enemigo se encuentra en 'Patrulla', desplazándose suavemente entre waypoints. 
> Cuando el jugador ingresa en su radio de visión y un Raycast confirma que no hay muros bloqueando la vista, el enemigo cambia instantáneamente al estado de 'Persecución', aumentando su velocidad para alcanzar al jugador. Si logramos escapar fuera de su radio de alcance, el guardián desiste y retoma su patrulla original."*

#### Minuto 3:45 - 4:45 | UI, Audio y Efectos Visuales (VFX)
> *"Nuestra interfaz de usuario o HUD muestra en tiempo real la cantidad de cristales recolectados, las vidas disponibles y el objetivo actual. También incorporamos un menú de pausa y pantallas modales de Game Over y Victoria.
> En el apartado audiovisual, integramos música de fondo ambiental y efectos de sonido para saltos, recolección, impacto y puntos de control. El portal final utiliza luces dinámicas y partículas que cambian de estado cuando todos los cristales han sido recuperados."*

#### Minuto 4:45 - 6:00 | Técnicas de Optimización (Requisito Especial)
> *"Pasando a las técnicas de optimización aplicadas:
> Primero, implementamos 'Object Pooling' mediante la clase `ObjectPool.cs`. En lugar de instanciar y destruir repetidamente objetos y efectos de partículas durante la partida (lo que sobrecarga el Garbage Collector de Unity generando caídas de frames), reutilizamos objetos preexistentes en una cola de memoria.
> Segundo, optimizamos los cálculos de Inteligencia Artificial aplicando un temporizador (Throttling) que recalcula la ruta del NavMesh cada 0.2 segundos en lugar de ejecutar `SetDestination` en cada fotograma, reduciendo drásticamente el consumo de CPU."*

#### Minuto 6:00 - 7:00 | Victoria, Ejecutable (.exe) y Conclusión
> *"Finalmente, al cruzar el portal activado, se desencadena la condición de Victoria, mostrando el tiempo empleado. Asimismo, si perdemos todas las vidas por caídas o ataques, se activa la condición de Game Over.
> Para concluir, mostramos aquí la ejecución del videojuego en su Build Standalone de Windows (.exe) corriendo con total fluidez fuera del editor de Unity.
> Muchas gracias por su atención."*
