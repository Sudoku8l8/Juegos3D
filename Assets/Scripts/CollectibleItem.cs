using UnityEngine;

/// <summary>
/// Mecánica de objeto recolectable (Cristales de Energía).
/// Incorpora animación procedural (rotación y flotación), atracción magnética hacia el jugador
/// y colisionador ampliado para garantizar que siempre esté al alcance.
/// </summary>
public class CollectibleItem : MonoBehaviour
{
    public enum CollectibleType
    {
        EnergyCrystal,
        HealthOrb
    }

    [Header("Tipo de Coleccionable")]
    [SerializeField] private CollectibleType itemType = CollectibleType.EnergyCrystal;
    [SerializeField] private int healValue = 1;

    [Header("Animación y Atracción")]
    [SerializeField] private float rotationSpeed = 85f;
    [SerializeField] private float bobbingSpeed = 2.5f;
    [SerializeField] private float bobbingHeight = 0.2f;
    [SerializeField] private float magnetDistance = 3.5f;
    [SerializeField] private float magnetSpeed = 9.0f;

    [Header("Efectos")]
    [Tooltip("Nombre de la etiqueta configurada en el ObjectPool para partículas")]
    [SerializeField] private string poolEffectTag = "CrystalCollectFX";
    [SerializeField] private GameObject fallbackParticlePrefab;

    private Vector3 initialPosition;
    private bool isCollected = false;
    private Transform playerTransform;

    private void Start()
    {
        initialPosition = transform.position;

        // Asegurar que el collider sea amplio y tipo Trigger para fácil recolección
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null)
        {
            sc = gameObject.AddComponent<SphereCollider>();
        }
        sc.isTrigger = true;
        sc.radius = Mathf.Max(sc.radius, 1.8f);

        // Localizar jugador para efecto imán
        playerController pc = FindFirstObjectByType<playerController>();
        if (pc != null)
        {
            playerTransform = pc.transform;
        }
    }

    private void Update()
    {
        if (isCollected) return;

        // 1. Rotación continua
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // 2. Atracción magnética si el jugador está cerca
        if (playerTransform != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position + Vector3.up * 0.8f);
            if (distToPlayer <= magnetDistance)
            {
                // Atraer el cristal hacia el pecho del jugador
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    playerTransform.position + Vector3.up * 0.8f,
                    magnetSpeed * Time.deltaTime
                );
                return;
            }
        }

        // 3. Flotación oscilatoria sinusoidal relajada
        float newY = initialPosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        playerController player = other.GetComponent<playerController>();
        if (player == null)
        {
            player = other.GetComponentInParent<playerController>();
        }

        if (player != null)
        {
            isCollected = true;
            ProcessCollection(player);
        }
    }

    private void ProcessCollection(playerController player)
    {
        if (itemType == CollectibleType.EnergyCrystal)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectCrystal();
            }
        }
        else if (itemType == CollectibleType.HealthOrb)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.Heal(healValue);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SoundType.Collect);
            }
        }

        SpawnCollectionFX();
        Destroy(gameObject);
    }

    private void SpawnCollectionFX()
    {
        // 1. Intentar usar Object Pooling
        if (ObjectPool.Instance != null && !string.IsNullOrEmpty(poolEffectTag))
        {
            GameObject pooledFX = ObjectPool.Instance.SpawnFromPool(poolEffectTag, transform.position, Quaternion.identity);
            if (pooledFX != null) return;
        }

        // 2. Respaldo por Instanciación directa
        if (fallbackParticlePrefab != null)
        {
            GameObject fx = Instantiate(fallbackParticlePrefab, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }
    }
}
