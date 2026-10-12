using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona la Interfaz de Usuario (Pantalla de Inicio con botón PLAY, HUD, Pausa, Victoria y Game Over).
/// Diseñado con arquitectura desacoplada mediante eventos de GameManager y PlayerHealth.
/// Incorpora sistema seguro contra referencias nulas y fallback visual OnGUI.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Pantalla de Inicio (Start / Play)")]
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private Button playButton;

    [Header("HUD Elements (TextMeshPro o UI Text)")]
    [SerializeField] private TextMeshProUGUI crystalsTextTMP;
    [SerializeField] private Text crystalsTextLegacy;

    [SerializeField] private TextMeshProUGUI healthTextTMP;
    [SerializeField] private Text healthTextLegacy;

    [SerializeField] private TextMeshProUGUI objectiveTextTMP;
    [SerializeField] private Text objectiveTextLegacy;

    [Header("Paneles de Estado")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Textos Informativos en Paneles")]
    [SerializeField] private TextMeshProUGUI victoryTimeTextTMP;
    [SerializeField] private Text victoryTimeTextLegacy;

    [Header("Botones de Control")]
    [SerializeField] private Button restartButtonVictory;
    [SerializeField] private Button nextLevelButtonVictory;
    [SerializeField] private Button restartButtonGameOver;
    [SerializeField] private Button resumeButtonPause;
    [SerializeField] private Button quitButton;

    // Cache interno para dibujo OnGUI de respaldo si no hay Canvas asignado
    private string cachedCrystalsStr = "Cristales: 0/5";
    private string cachedHealthStr = "Salud: 3/3 | Vidas: 3";
    private string cachedObjectiveStr = "Misión: Recolecta todos los cristales para activar el portal";
    private bool showFallbackGUI = false;

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
        // Estado inicial de los paneles
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        bool inStartMenu = (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.StartMenu);
        if (startMenuPanel != null) startMenuPanel.SetActive(inStartMenu);
        if (hudPanel != null) hudPanel.SetActive(!inStartMenu);

        // Si no hay textos asignados en el inspector, activar OnGUI de respaldo
        if (crystalsTextTMP == null && crystalsTextLegacy == null)
        {
            showFallbackGUI = true;
        }

        SetupButtons();
    }

    private void OnEnable()
    {
        GameManager.OnCrystalsUpdated += UpdateCrystalsHUD;
        GameManager.OnPortalUnlocked += ShowPortalUnlockedHUD;
        GameManager.OnGameWon += ShowVictoryScreen;
        GameManager.OnGameOverEvent += ShowGameOverScreen;
        GameManager.OnPauseToggled += ShowPauseScreen;
        GameManager.OnStartMenuToggled += HandleStartMenuToggled;

        PlayerHealth.OnHealthChanged += UpdateHealthHUD;
        PlayerHealth.OnLivesChanged += UpdateLivesHUD;
    }

    private void OnDisable()
    {
        GameManager.OnCrystalsUpdated -= UpdateCrystalsHUD;
        GameManager.OnPortalUnlocked -= ShowPortalUnlockedHUD;
        GameManager.OnGameWon -= ShowVictoryScreen;
        GameManager.OnGameOverEvent -= ShowGameOverScreen;
        GameManager.OnPauseToggled -= ShowPauseScreen;
        GameManager.OnStartMenuToggled -= HandleStartMenuToggled;

        PlayerHealth.OnHealthChanged -= UpdateHealthHUD;
        PlayerHealth.OnLivesChanged -= UpdateLivesHUD;
    }

    private void SetupButtons()
    {
        if (playButton != null)
            playButton.onClick.AddListener(() => GameManager.Instance.StartGame());

        if (restartButtonVictory != null)
            restartButtonVictory.onClick.AddListener(() => GameManager.Instance.RestartLevel());

        if (nextLevelButtonVictory != null)
            nextLevelButtonVictory.onClick.AddListener(() => GameManager.Instance.LoadNextLevel());

        if (restartButtonGameOver != null)
            restartButtonGameOver.onClick.AddListener(() => GameManager.Instance.RestartLevel());

        if (resumeButtonPause != null)
            resumeButtonPause.onClick.AddListener(() => GameManager.Instance.TogglePause());

        if (quitButton != null)
            quitButton.onClick.AddListener(() => GameManager.Instance.QuitGame());
    }

    private void HandleStartMenuToggled(bool show)
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(show);
        if (hudPanel != null) hudPanel.SetActive(!show);
    }

    #region Actualizaciones de HUD
    private void UpdateCrystalsHUD(int current, int total)
    {
        cachedCrystalsStr = $"Cristales: {current} / {total}";

        if (current >= total && total > 0)
        {
            cachedObjectiveStr = "¡PORTAL ACTIVADO! Corre hacia la salida brillante.";
        }
        else
        {
            int remaining = total - current;
            cachedObjectiveStr = $"Misión: Recolecta {remaining} cristales más para abrir el portal";
        }

        if (crystalsTextTMP != null) crystalsTextTMP.text = cachedCrystalsStr;
        if (crystalsTextLegacy != null) crystalsTextLegacy.text = cachedCrystalsStr;
        if (objectiveTextTMP != null) objectiveTextTMP.text = cachedObjectiveStr;
        if (objectiveTextLegacy != null) objectiveTextLegacy.text = cachedObjectiveStr;
    }

    private void UpdateHealthHUD(int current, int max)
    {
        cachedHealthStr = $"Salud: {current}/{max}";
        if (healthTextTMP != null) healthTextTMP.text = cachedHealthStr;
        if (healthTextLegacy != null) healthTextLegacy.text = cachedHealthStr;
    }

    private void UpdateLivesHUD(int currentLives, int maxLives)
    {
        string livesStr = $" | Vidas: {currentLives}";
        cachedHealthStr += livesStr;
        if (healthTextTMP != null) healthTextTMP.text = cachedHealthStr;
        if (healthTextLegacy != null) healthTextLegacy.text = cachedHealthStr;
    }

    private void ShowPortalUnlockedHUD()
    {
        cachedObjectiveStr = "¡PORTAL ACTIVADO! Corre hacia la salida.";
        if (objectiveTextTMP != null) objectiveTextTMP.text = cachedObjectiveStr;
        if (objectiveTextLegacy != null) objectiveTextLegacy.text = cachedObjectiveStr;
    }
    #endregion

    #region Paneles Modales
    private void ShowVictoryScreen(float timeTaken)
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(true);

        int minutes = Mathf.FloorToInt(timeTaken / 60F);
        int seconds = Mathf.FloorToInt(timeTaken - minutes * 60);
        string timeStr = $"Tiempo de Escape: {minutes:00}:{seconds:00}";

        if (victoryTimeTextTMP != null) victoryTimeTextTMP.text = timeStr;
        if (victoryTimeTextLegacy != null) victoryTimeTextLegacy.text = timeStr;
    }

    private void ShowGameOverScreen()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    private void ShowPauseScreen(bool isPaused)
    {
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }
    #endregion

    #region Fallback OnGUI
    private void OnGUI()
    {
        if (!showFallbackGUI) return;

        GUIStyle headerStyle = new GUIStyle(GUI.skin.box);
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.white;

        // 1. Pantalla de Inicio / Menú Principal (Botón PLAY)
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.StartMenu)
        {
            float width = 450f;
            float height = 300f;
            Rect rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);

            GUI.Box(rect, "<b>CRYSTAL RELIC: TEMPLE ESCAPE</b>");
            GUILayout.BeginArea(new Rect(rect.x + 25, rect.y + 35, width - 50, height - 50));
            
            GUILayout.Space(10);
            GUILayout.Label("¡Bienvenido, explorador!\nControla al niño para recolectar los 5 cristales de energía, evadir al guardián y activar el portal de escape.", headerStyle);
            
            GUILayout.Space(25);
            GUI.backgroundColor = new Color(0.2f, 0.85f, 0.3f);
            if (GUILayout.Button("▶ JUGAR / PLAY", GUILayout.Height(50)))
            {
                GameManager.Instance.StartGame();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.Space(10);
            if (GUILayout.Button("Salir del Juego", GUILayout.Height(30)))
            {
                GameManager.Instance.QuitGame();
            }

            GUILayout.EndArea();
            return;
        }

        // 2. HUD Superior Izquierdo durante el Gameplay
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            GUILayout.BeginArea(new Rect(20, 20, 390, 115), GUI.skin.box);
            GUILayout.Label($"<b>[ESTADO]</b> {cachedHealthStr}");
            GUILayout.Label($"<b>[OBJETIVO]</b> {cachedCrystalsStr}");
            if (GameManager.Instance.CurrentCrystals >= GameManager.Instance.RequiredCrystals)
            {
                GUILayout.Label("<color=#00FFFF><b>¡PORTAL ACTIVADO! CRUZA LA SALIDA</b></color>");
            }
            else
            {
                GUILayout.Label($"<color=yellow>{cachedObjectiveStr}</color>");
            }
            GUILayout.EndArea();
        }

        // 3. Pantalla de Victoria Fallback
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Victory)
        {
            DrawModalWindow("¡VICTORIA! - Has completado el nivel", () =>
            {
                GUILayout.Label($"Tiempo total: {GameManager.Instance.ElapsedTime:F1} segundos", headerStyle);
                GUILayout.Space(10);

                if (GameManager.Instance.HasNextLevel())
                {
                    GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
                    if (GUILayout.Button("Siguiente Nivel ▶", GUILayout.Height(45)))
                    {
                        GameManager.Instance.LoadNextLevel();
                    }
                    GUI.backgroundColor = Color.white;
                }

                if (GUILayout.Button("Reintentar Nivel", GUILayout.Height(35))) GameManager.Instance.RestartLevel();
                if (GUILayout.Button("Salir del Juego", GUILayout.Height(30))) GameManager.Instance.QuitGame();
            });
        }

        // 4. Pantalla de Game Over Fallback
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
        {
            DrawModalWindow("GAME OVER - Has sido derrotado", () =>
            {
                GUILayout.Label("Te has quedado sin vidas.", headerStyle);
                GUILayout.Space(10);
                if (GUILayout.Button("Reintentar Nivel", GUILayout.Height(45))) GameManager.Instance.RestartLevel();
                if (GUILayout.Button("Salir del Juego", GUILayout.Height(30))) GameManager.Instance.QuitGame();
            });
        }

        // 5. Pantalla de Pausa Fallback
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Paused)
        {
            DrawModalWindow("JUEGO EN PAUSA", () =>
            {
                if (GUILayout.Button("Reanudar", GUILayout.Height(40))) GameManager.Instance.TogglePause();
                if (GUILayout.Button("Reiniciar Nivel", GUILayout.Height(30))) GameManager.Instance.RestartLevel();
                if (GUILayout.Button("Salir", GUILayout.Height(30))) GameManager.Instance.QuitGame();
            });
        }
    }

    private void DrawModalWindow(string title, System.Action content)
    {
        float width = 380f;
        float height = 250f;
        Rect rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);

        GUI.Box(rect, $"<b>{title}</b>");
        GUILayout.BeginArea(new Rect(rect.x + 20, rect.y + 35, width - 40, height - 45));
        content?.Invoke();
        GUILayout.EndArea();
    }
    #endregion
}
