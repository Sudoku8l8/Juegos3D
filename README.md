# 🎮 Crystal Relic: Temple Escape (Proyecto Capstone 3D)

Videojuego 3D interactivo desarrollado en **Unity 6** y **C#** como proyecto Capstone. Cumple rigurosamente con los criterios de evaluación: entorno 3D, ciclo de juego completo, objetivo y condiciones de victoria/derrota, Inteligencia Artificial (Patrulla y Persecución), UI interactiva, Audio, Efectos Visuales (VFX) y Técnicas de Optimización formales.

---

## 📋 Resumen del Proyecto

* **Motor:** Unity 6 (versión 6000.6.5f1)
* **Plataforma:** PC (Windows Standalone x86_64)
* **Género:** Plataformas 3D / Aventura / Escape
* **Duración:** 3 a 5 minutos de gameplay
* **Entrada de controles:** New Input System (Teclado y Mouse)

---

## 🕹️ Controles de Juego

| Acción | Tecla / Control |
| :--- | :--- |
| **Moverse** | `W`, `A`, `S`, `D` / Flechas direccionales |
| **Saltar** | Barra espaciadora (`Space`) |
| **Rotar Cámara** | Movimiento del Mouse |
| **Pausar / Desbloquear Cursor** | `ESC` o tecla `P` |
| **Volver a fijar el Mouse** | Clic izquierdo |

---

## 🎯 Objetivo y Mecánicas

1. **Exploración de Plataformas:** Desplázate por el escenario esquivando trampas de púas y caídas al vacío.
2. **Puntos de Control (Checkpoints):** Toca los altares para cambiar su color a verde y guardar tu punto de reaparición.
3. **Recolección de Cristales (Mecánica Principal):** Encuentra los **5 Cristales de Energía** repartidos por el nivel.
4. **Enemigo Guardián (IA):** Evade al enemigo ancestral que patrulla la zona central y te perseguirá al detectar tu presencia.
5. **Condición de Victoria:** Una vez reunidos los 5 cristales, dirígete al **Portal de Salida** para escapar del templo y registrar tu tiempo.
6. **Condición de Derrota:** Cuentas con **3 Vidas**. Si agotas tus vidas por golpes de enemigos o trampas, se despliega la pantalla de Game Over.

---

## ⚙️ Arquitectura de Software y Sistemas

* **`GameManager.cs`:** Singleton que gobierna el Game Loop (Playing, Paused, Victory, GameOver).
* **`PlayerHealth.cs`:** Control de vidas, salud, daño por contacto, caída al abismo y parpadeo de invulnerabilidad.
* **`EnemyAI.cs`:** Máquina de estados finitos (FSM) con comportamientos de **Patrulla (Waypoints)** y **Persecución (Chase)** mediante `NavMeshAgent` y detección por `Raycast`.
* **`CollectibleItem.cs`:** Animación procedural oscilatoria (rotación + flotación seno), sonido y partículas al recolectar.
* **`ExitPortal.cs`:** Escucha el evento de activación de portal, despliega iluminación/partículas y activa la victoria.
* **`UIManager.cs`:** HUD en pantalla (vidas, cristales, objetivo actual) y pantallas modales de Victoria, Game Over y Pausa.
* **`AudioManager.cs`:** Música de fondo en bucle y efectos sonoros espaciales y 2D (con respaldo procedural automático).
* **`ObjectPool.cs`:** Técnica de optimización para reciclaje de instancias sin sobrecargar el Garbage Collector.

---

## 🚀 Técnicas de Optimización Implementadas

1. **Object Pooling:** Reutilización de GameObjects en cola (`ObjectPool.cs`) para efectos de partículas y feedback, evitando picos de recolección de basura (*Garbage Collection*).
2. **Throttling en Cálculos de IA:** El script `EnemyAI.cs` recalcula la ruta del NavMesh cada `0.2s` mediante un temporizador, reduciendo en más del 85% el costo de CPU frente a la ejecución continua en `Update()`.
3. **LOD (Level of Detail):** Uso de `LODGroup` en los prefabs del escenario para reducir la carga de renderizado poligonal según la distancia a la cámara.

---

## 🛠️ Cómo Abrir y Probar en Unity

1. Abre el proyecto en **Unity Hub** con la versión **Unity 6 (6000.6.5f1)**.
2. Abre la escena principal ubicada en `Assets/Scenes/MainScene.unity`.
3. Para configurar automáticamente todos los sistemas en 1 clic:
   * En la barra superior de Unity, ve a: **Tools > Capstone 3D > Configurar Escena Automáticamente**.
4. Presiona el botón **Play (▶)** para jugar inmediatamente.

---

## 📦 Cómo Generar el Build Ejecutable (.exe)

1. En Unity, ve a `File > Build Settings...`.
2. Asegúrate de que `MainScene` esté añadida en la lista **Scenes in Build** con el índice `0`.
3. Selecciona la plataforma **Windows, Mac, Linux** y como objetivo **Windows (x86_64)**.
4. Presiona **Build**, selecciona una carpeta llamada `Build/` y espera a que finalice la compilación.
5. Ejecuta el archivo `.exe` generado fuera de Unity para validar su rendimiento.

---

## 📄 Documentación Incluida

* [Informe Técnico Completo (PDF 4-6 págs)](Docs/INFORME_PROYECTO_CAPSTONE.md)
* [Guion Técnico del Video Demostrativo (5-7 mins)](Docs/GUION_VIDEO_DEMOSTRATIVO.md)
