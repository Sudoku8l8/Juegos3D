using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador principal del ciclo de vida del juego (Game Loop).
/// Administra estados (Inicio/Play, Gameplay, Pausa, Victoria y Derrota),
/// progresión multicanal de 3 niveles y eventos del sistema.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        StartMenu,
        Playing,
        Paused,
        Victory,
        GameOver
    }

    [Header("Configuración de Inicio")]
    [SerializeField] private bool showStartMenuOnLoad = true;

    [Header("Objetivos del Nivel")]
    [Tooltip("Cantidad total de cristales necesarios para abrir el portal")]
    [SerializeField] private int requiredCrystals = 5;

    public int RequiredCrystals => requiredCrystals;
    public int CurrentCrystals { get; private set; } = 0;
    public GameState CurrentState { get; private set; } = GameState.StartMenu;
    public float ElapsedTime { get; private set; } = 0f;

    // Nombres oficiales de los niveles
    public static readonly string[] LevelOrder = { "MainScene", "Level_2", "Level_3" };

    // Eventos desacoplados para la UI y otros sistemas
    public static event Action<int, int> OnCrystalsUpdated;
    public static event Action OnPortalUnlocked;
    public static event Action<float> OnGameWon;
    public static event Action OnGameOverEvent;
    public static event Action<bool> OnPauseToggled;
    public static event Action<bool> OnStartMenuToggled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ElapsedTime = 0f;
        CurrentCrystals = 0;

        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isFirstScene = (activeScene == "MainScene" || activeScene == LevelOrder[0]);

        // Mostrar pantalla de inicio solo en el primer nivel
        if (showStartMenuOnLoad && isFirstScene)
        {
            CurrentState = GameState.StartMenu;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            OnStartMenuToggled?.Invoke(true);
        }
        else
        {
            // En Level_2 y Level_3 arranca directamente el gameplay
            StartGame();
        }

        // Sincronizar requiredCrystals con la cantidad real en la escena (se colocan por GameInitializer)
        // Se reintenta con un pequeño delay si los cristales aun no existen en el momento de Start
        CollectibleItem[] itemsInScene = FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None);
        if (itemsInScene.Length > 0)
        {
            requiredCrystals = itemsInScene.Length;
        }

        OnCrystalsUpdated?.Invoke(CurrentCrystals, requiredCrystals);
        Invoke(nameof(RefreshRequiredCrystals), 0.5f);
    }

    /// <summary>Refresca requiredCrystals medio segundo despues para capturar objetos creados tarde por GameInitializer.</summary>
    private void RefreshRequiredCrystals()
    {
        CollectibleItem[] itemsInScene = FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None);
        if (itemsInScene.Length > 0 && itemsInScene.Length != requiredCrystals)
        {
            requiredCrystals = itemsInScene.Length;
            OnCrystalsUpdated?.Invoke(CurrentCrystals, requiredCrystals);
        }
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            ElapsedTime += Time.deltaTime;
        }

        // Tecla de pausa (Escape o P) solo durante el juego
        if (CurrentState == GameState.Playing || CurrentState == GameState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
        }
    }

    /// <summary>
    /// Inicia la partida desde la pantalla de bienvenida al hacer clic en PLAY.
    /// </summary>
    public void StartGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnStartMenuToggled?.Invoke(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic();
            AudioManager.Instance.PlaySFX(AudioManager.SoundType.ButtonClick);
        }

        Debug.Log("[GameManager] ¡Partida iniciada! Que comience la aventura.");
    }

    /// <summary>
    /// Registra la recolección de un cristal y evalúa la activación del portal.
    /// </summary>
    public void CollectCrystal()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentCrystals++;
        OnCrystalsUpdated?.Invoke(CurrentCrystals, requiredCrystals);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SoundType.Collect);
        }

        if (CurrentCrystals >= requiredCrystals)
        {
            Debug.Log("[GameManager] ¡Todos los cristales recolectados! El portal se ha activado.");
            OnPortalUnlocked?.Invoke();

            ExitPortal portal = FindFirstObjectByType<ExitPortal>();
            if (portal != null)
            {
                portal.UnlockPortal();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SoundType.PortalActive);
            }
        }
    }

    /// <summary>
    /// Ejecuta la condición de Victoria al alcanzar el portal de salida.
    /// </summary>
    public void TriggerVictory()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Victory;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SoundType.Victory);
        }

        OnGameWon?.Invoke(ElapsedTime);
        Debug.Log($"[GameManager] ¡Victoria! Nivel completado en {ElapsedTime:F2} segundos.");
    }

    /// <summary>
    /// Ejecuta la condición de Derrota cuando el jugador pierde todas sus vidas o salud.
    /// </summary>
    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SoundType.GameOver);
        }

        OnGameOverEvent?.Invoke();
        Debug.Log("[GameManager] Fin de la partida (Game Over).");
    }

    /// <summary>
    /// Alterna el estado de pausa del juego.
    /// </summary>
    public void TogglePause()
    {
        if (CurrentState == GameState.Victory || CurrentState == GameState.GameOver || CurrentState == GameState.StartMenu) return;

        bool isPausing = (CurrentState == GameState.Playing);
        CurrentState = isPausing ? GameState.Paused : GameState.Playing;
        Time.timeScale = isPausing ? 0f : 1f;

        Cursor.lockState = isPausing ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPausing;

        OnPauseToggled?.Invoke(isPausing);
    }

    /// <summary>
    /// Carga el siguiente nivel de los 3 niveles del juego.
    /// </summary>
    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        string activeSceneName = SceneManager.GetActiveScene().name;

        int currentIndex = Array.IndexOf(LevelOrder, activeSceneName);
        if (currentIndex >= 0 && currentIndex + 1 < LevelOrder.Length)
        {
            string nextScene = LevelOrder[currentIndex + 1];
            Debug.Log($"[GameManager] Cargando siguiente nivel: {nextScene}");
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            // Si ya terminó el nivel 3, reiniciar al nivel 1
            Debug.Log("[GameManager] ¡Has completado todos los niveles! Reiniciando al Nivel 1.");
            SceneManager.LoadScene(LevelOrder[0]);
        }
    }

    public bool HasNextLevel()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        int currentIndex = Array.IndexOf(LevelOrder, activeSceneName);
        return (currentIndex >= 0 && currentIndex + 1 < LevelOrder.Length);
    }

    /// <summary>
    /// Reinicia la escena actual.
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Cierra la aplicación (funciona tanto en Build como en Editor).
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
