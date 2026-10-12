using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Gestiona la salud, vidas y daño del jugador.
/// Proporciona frames de invulnerabilidad (i-frames) y respuesta visual al recibir impactos.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Salud y Vidas")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float invulnerabilityDuration = 1.2f;
    [SerializeField] private float fallDeathThreshold = -15f;

    private int currentHealth;
    private int currentLives;
    private bool isInvulnerable = false;
    private playerController movementController;
    private System.Collections.Generic.List<Renderer> validRenderers = new System.Collections.Generic.List<Renderer>();

    public static event Action<int, int> OnHealthChanged;
    public static event Action<int, int> OnLivesChanged;

    private void Awake()
    {
        movementController = GetComponent<playerController>();

        // Si el objeto raíz tiene una cápsula azul desactivada (placeholder de Unity), la eliminamos
        MeshRenderer rootMesh = GetComponent<MeshRenderer>();
        if (rootMesh != null && !rootMesh.enabled)
        {
            Destroy(rootMesh);
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf != null) Destroy(mf);
        }

        // Registrar únicamente los renderers activos del modelo 3D del niño
        Renderer[] all = GetComponentsInChildren<Renderer>(false);
        foreach (Renderer r in all)
        {
            if (r != null && r.gameObject != gameObject && r.enabled)
            {
                validRenderers.Add(r);
            }
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentLives = maxLives;

        // Notificar valores iniciales
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnLivesChanged?.Invoke(currentLives, maxLives);
    }

    private void Update()
    {
        // Detección automática de caída al vacío
        if (transform.position.y < fallDeathThreshold)
        {
            HandleFallDeath();
        }
    }

    /// <summary>
    /// Aplica daño al jugador reduciendo su salud o vidas.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.SoundType.Damage);
        }

        if (currentHealth <= 0)
        {
            HandleLifeLost();
        }
        else
        {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    /// <summary>
    /// Recupera salud del jugador.
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void HandleLifeLost()
    {
        currentLives--;
        OnLivesChanged?.Invoke(currentLives, maxLives);

        if (currentLives <= 0)
        {
            // Derrota definitiva
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
        else
        {
            // Reaparecer en el último checkpoint
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (movementController != null)
            {
                movementController.Die();
            }

            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    private void HandleFallDeath()
    {
        HandleLifeLost();
    }

    /// <summary>
    /// Parpadeo visual durante el periodo de invulnerabilidad.
    /// </summary>
    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;
        float blinkRate = 0.1f;

        while (elapsed < invulnerabilityDuration)
        {
            SetRenderersVisible(false);
            yield return new WaitForSeconds(blinkRate);
            SetRenderersVisible(true);
            yield return new WaitForSeconds(blinkRate);

            elapsed += blinkRate * 2f;
        }

        SetRenderersVisible(true);
        isInvulnerable = false;
    }

    private void SetRenderersVisible(bool visible)
    {
        if (validRenderers == null) return;
        foreach (Renderer rend in validRenderers)
        {
            if (rend != null) rend.enabled = visible;
        }
    }
}
