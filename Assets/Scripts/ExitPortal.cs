using UnityEngine;

/// <summary>
/// Mecanismo de Portal de Salida que define la Condición de Victoria.
/// Comienza inactivo (color rojo/gris) y se desbloquea espectacularmente (color cian radiante y partículas)
/// cuando el jugador recoge todos los cristales requeridos.
/// </summary>
public class ExitPortal : MonoBehaviour
{
    [Header("Componentes Visuales")]
    [SerializeField] private ParticleSystem activeParticles;
    [SerializeField] private Light portalLight;
    [SerializeField] private Renderer portalRenderer;
    [SerializeField] private Color lockedColor = new Color(0.7f, 0.2f, 0.2f, 0.8f); // Rojo tenue
    [SerializeField] private Color unlockedColor = new Color(0.1f, 0.9f, 1.0f, 1.0f); // Cian brillante

    [Header("Estado")]
    [SerializeField] private bool isUnlocked = false;

    private Renderer[] allChildRenderers;
    private float pulseTimer = 0f;

    public bool IsUnlocked => isUnlocked;

    private void Awake()
    {
        // Autodetección robusta de componentes si no se asignaron en el inspector
        if (portalRenderer == null) portalRenderer = GetComponent<Renderer>();
        if (portalLight == null) portalLight = GetComponentInChildren<Light>();
        if (activeParticles == null) activeParticles = GetComponentInChildren<ParticleSystem>();
        allChildRenderers = GetComponentsInChildren<Renderer>();

        // Crear sistema de partículas de respaldo si no existe
        if (activeParticles == null)
        {
            GameObject pObj = new GameObject("PortalEnergyParticles");
            pObj.transform.SetParent(transform);
            pObj.transform.localPosition = Vector3.up * 1.5f;
            activeParticles = pObj.AddComponent<ParticleSystem>();

            var main = activeParticles.main;
            main.startColor = unlockedColor;
            main.startSize = 0.4f;
            main.startSpeed = 2.5f;
            main.loop = true;
            main.playOnAwake = false;

            var emission = activeParticles.emission;
            emission.rateOverTime = 25f;
        }

        SetPortalState(false);
    }

    private void Start()
    {
        // Comprobar si al iniciar ya se cumplió el objetivo
        if (GameManager.Instance != null && GameManager.Instance.CurrentCrystals >= GameManager.Instance.RequiredCrystals)
        {
            UnlockPortal();
        }
    }

    private void Update()
    {
        // Verificacion continua: desbloquear si ya se tienen todos los cristales
        if (!isUnlocked && GameManager.Instance != null &&
            GameManager.Instance.RequiredCrystals > 0 &&
            GameManager.Instance.CurrentCrystals >= GameManager.Instance.RequiredCrystals)
        {
            UnlockPortal();
        }

        // Efectos visuales cuando el portal esta abierto
        if (isUnlocked)
        {
            pulseTimer += Time.deltaTime;

            // Pulsar la luz
            if (portalLight != null)
            {
                portalLight.intensity = 4.0f + Mathf.Sin(pulseTimer * 3f) * 1.5f;
            }

            // Pulsar la esfera central de energia
            Transform orbT = transform.Find("PortalOrb");
            if (orbT != null)
            {
                float orbScale = 0.9f + Mathf.Sin(pulseTimer * 4f) * 0.15f;
                orbT.localScale = Vector3.one * orbScale;
            }
        }
    }

    private void OnEnable()
    {
        GameManager.OnPortalUnlocked += UnlockPortal;
    }

    private void OnDisable()
    {
        GameManager.OnPortalUnlocked -= UnlockPortal;
    }

    public void UnlockPortal()
    {
        if (isUnlocked) return;
        Debug.Log("[ExitPortal] ¡¡¡PORTAL DESBLOQUEADO CON ÉXITO!!! Los cristales han sido energizados.");
        SetPortalState(true);
    }

    private void SetPortalState(bool active)
    {
        isUnlocked = active;

        // 1. Particulas
        if (activeParticles != null)
        {
            if (active) activeParticles.Play();
            else activeParticles.Stop();
        }

        // 2. Buscar y activar sistema de particulas hijo (creado por GameInitializer)
        ParticleSystem[] childPS = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in childPS)
        {
            if (active) ps.Play(); else ps.Stop();
        }

        // 3. Luz principal
        if (portalLight != null)
        {
            portalLight.enabled   = true;
            portalLight.color     = active ? unlockedColor : lockedColor;
            portalLight.intensity = active ? 5.0f : 1.5f;
            portalLight.range     = active ? 18.0f : 7.0f;
        }

        // 4. Buscar la luz interna generada dinamicamente
        Light[] childLights = GetComponentsInChildren<Light>();
        foreach (Light l in childLights)
        {
            l.color     = active ? unlockedColor : lockedColor;
            l.intensity = active ? 5.0f : 1.5f;
        }

        // 5. Aplicar color a todos los renderizadores hijos
        Color targetColor = active ? unlockedColor : lockedColor;
        allChildRenderers = GetComponentsInChildren<Renderer>();
        if (allChildRenderers != null)
        {
            foreach (Renderer r in allChildRenderers)
            {
                if (r != null && r.material != null)
                {
                    r.material.color = targetColor;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        playerController player = other.GetComponent<playerController>();
        if (player == null)
        {
            player = other.GetComponentInParent<playerController>();
        }

        if (player != null)
        {
            if (isUnlocked)
            {
                Debug.Log("[ExitPortal] ¡El niño cruzó el portal! Condición de Victoria alcanzada.");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerVictory();
                }
            }
            else
            {
                int current = GameManager.Instance != null ? GameManager.Instance.CurrentCrystals : 0;
                int total = GameManager.Instance != null ? GameManager.Instance.RequiredCrystals : 5;
                Debug.Log($"[ExitPortal] Portal aún bloqueado. Llevas {current}/{total} cristales.");
            }
        }
    }
}
