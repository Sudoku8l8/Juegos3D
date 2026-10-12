using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Inicializador automatico del juego (Runtime Bootstrap).
/// Detecta la escena activa y coloca cristales, portal y enemigos
/// con coordenadas especificas para MainScene, Level_2 y Level_3.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    // MainScene
    private static readonly Vector3[] MainScene_Crystals = new Vector3[]
    {
        new Vector3(-1.83f,  16.7f, 15.52f),
        new Vector3(-1.83f,  16.7f, 24.63f),
        new Vector3(-8.36f,  11.7f, 32.77f),
        new Vector3(-19.50f, 14.1f, 34.36f),
        new Vector3(-36.88f,  4.6f, 32.53f)
    };
    private static readonly Vector3 MainScene_Portal = new Vector3(-36.26f, 3.6f, 42.35f);
    private static readonly Vector3 MainScene_Enemy  = new Vector3(-12.0f, 11.5f, 30.0f);

    // Level_2
    private static readonly Vector3[] Level2_Crystals = new Vector3[]
    {
        new Vector3(  5.0f, 3.0f,  5.0f),
        new Vector3( 15.0f, 3.0f,  0.0f),
        new Vector3(  0.0f, 3.0f, 20.0f),
        new Vector3(-15.0f, 3.0f, 10.0f),
        new Vector3( 10.0f, 3.0f, 30.0f)
    };
    private static readonly Vector3 Level2_Portal = new Vector3( 0.0f, 3.0f, 40.0f);
    private static readonly Vector3 Level2_Enemy  = new Vector3( 5.0f, 3.0f, 15.0f);

    // Level_3
    private static readonly Vector3[] Level3_Crystals = new Vector3[]
    {
        new Vector3(  3.0f, 3.0f,  3.0f),
        new Vector3(-10.0f, 3.0f, 10.0f),
        new Vector3( 12.0f, 3.0f, 18.0f),
        new Vector3( -5.0f, 3.0f, 25.0f),
        new Vector3(  8.0f, 3.0f, 38.0f)
    };
    private static readonly Vector3 Level3_Portal = new Vector3( 0.0f, 3.0f, 50.0f);
    private static readonly Vector3 Level3_Enemy  = new Vector3(-8.0f, 3.0f, 20.0f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitializeScene()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene != "MainScene" && scene != "Level_2" && scene != "Level_3") return;

        // Sistemas globales (solo se crean una vez por DontDestroyOnLoad)
        GameObject managersObj = GameObject.Find("[GAME_SYSTEMS]");
        if (managersObj == null)
        {
            managersObj = new GameObject("[GAME_SYSTEMS]");
            DontDestroyOnLoad(managersObj);
        }

        if (FindFirstObjectByType<GameManager>() == null)
            managersObj.AddComponent<GameManager>();

        if (FindFirstObjectByType<AudioManager>() == null)
            managersObj.AddComponent<AudioManager>();

        if (FindFirstObjectByType<UIManager>() == null)
            managersObj.AddComponent<UIManager>();

        if (FindFirstObjectByType<ObjectPool>() == null)
            managersObj.AddComponent<ObjectPool>();

        ConfigurePlayer();

        Vector3[] crystalPositions;
        Vector3   portalPos;
        Vector3   enemyPos;

        switch (scene)
        {
            case "Level_2":
                crystalPositions = Level2_Crystals;
                portalPos        = Level2_Portal;
                enemyPos         = Level2_Enemy;
                break;
            case "Level_3":
                crystalPositions = Level3_Crystals;
                portalPos        = Level3_Portal;
                enemyPos         = Level3_Enemy;
                break;
            default:
                crystalPositions = MainScene_Crystals;
                portalPos        = MainScene_Portal;
                enemyPos         = MainScene_Enemy;
                break;
        }

        ConfigureCollectibles(crystalPositions);
        ConfigureExitPortal(portalPos);
        ConfigureEnemyAI(enemyPos);
        ConfigureGroundHazard();

        Debug.Log($"[GameInitializer] Escena '{scene}' inicializada.");
    }

    private static void ConfigurePlayer()
    {
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj == null)
        {
            playerController pc = FindFirstObjectByType<playerController>();
            if (pc != null) playerObj = pc.gameObject;
        }
        if (playerObj != null)
        {
            if (!playerObj.CompareTag("Player")) playerObj.tag = "Player";
            if (playerObj.GetComponent<PlayerHealth>() == null)
                playerObj.AddComponent<PlayerHealth>();
        }
    }

    private static void ConfigureCollectibles(Vector3[] positions)
    {
        CollectibleItem[] existing = FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None);
        if (existing.Length != positions.Length)
        {
            foreach (CollectibleItem c in existing) UnityEngine.Object.Destroy(c.gameObject);
            for (int i = 0; i < positions.Length; i++)
                CreateDynamicCrystal(positions[i], $"Crystal_{i + 1}");
            Debug.Log($"[GameInitializer] {positions.Length} cristales colocados.");
        }
    }

    private static GameObject CreateDynamicCrystal(Vector3 position, string objName)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 8f, Vector3.down, out hit, 20f))
            position = hit.point + Vector3.up * 1.0f;

        GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crystal.name = objName;
        crystal.transform.position   = position;
        crystal.transform.localScale = new Vector3(0.6f, 1.1f, 0.6f);

        SphereCollider col = crystal.GetComponent<SphereCollider>();
        if (col != null) { col.isTrigger = true; col.radius = 2.0f; }

        crystal.AddComponent<CollectibleItem>();

        Renderer rend = crystal.GetComponent<Renderer>();
        if (rend != null) rend.material.color = new Color(1.0f, 0.85f, 0.1f);

        GameObject lightObj = new GameObject("GlowLight");
        lightObj.transform.SetParent(crystal.transform);
        lightObj.transform.localPosition = Vector3.zero;
        Light glow = lightObj.AddComponent<Light>();
        glow.type      = LightType.Point;
        glow.range     = 6f;
        glow.color     = new Color(1f, 0.9f, 0.3f);
        glow.intensity = 3f;

        return crystal;
    }

    private static void ConfigureExitPortal(Vector3 rawPos)
    {
        if (FindFirstObjectByType<ExitPortal>() != null) return;

        Vector3 pos = rawPos;
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up * 10f, Vector3.down, out hit, 25f))
            pos = hit.point + Vector3.up * 0.05f;

        GameObject portalRoot = new GameObject("ExitPortal_Goal");
        portalRoot.transform.position = pos;

        // Base
        GameObject baseDisk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseDisk.name = "PortalBase";
        baseDisk.transform.SetParent(portalRoot.transform);
        baseDisk.transform.localPosition = Vector3.zero;
        baseDisk.transform.localScale    = new Vector3(4f, 0.15f, 4f);
        SetColor(baseDisk, new Color(0.05f, 0.3f, 0.9f));
        UnityEngine.Object.Destroy(baseDisk.GetComponent<Collider>());

        // Pilares
        CreatePortalPillar(portalRoot, new Vector3(-1.8f, 2.5f, 0f));
        CreatePortalPillar(portalRoot, new Vector3( 1.8f, 2.5f, 0f));

        // Arco superior
        GameObject arch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arch.name = "PortalArch";
        arch.transform.SetParent(portalRoot.transform);
        arch.transform.localPosition = new Vector3(0f, 5.0f, 0f);
        arch.transform.localScale    = new Vector3(0.35f, 2.0f, 0.35f);
        arch.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        SetColor(arch, new Color(0.1f, 0.7f, 1.0f));
        UnityEngine.Object.Destroy(arch.GetComponent<Collider>());

        // Vortice central
        GameObject vortex = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        vortex.name = "PortalVortex";
        vortex.transform.SetParent(portalRoot.transform);
        vortex.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        vortex.transform.localScale    = new Vector3(2.8f, 0.06f, 2.8f);
        vortex.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        SetColor(vortex, new Color(0.0f, 0.6f, 1.0f));
        UnityEngine.Object.Destroy(vortex.GetComponent<Collider>());

        // Anillo 1 (azul, gira lento)
        GameObject ring1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring1.name = "PortalRing1";
        ring1.transform.SetParent(portalRoot.transform);
        ring1.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        ring1.transform.localScale    = new Vector3(3.8f, 0.06f, 3.8f);
        ring1.transform.localRotation = Quaternion.Euler(80f, 0f, 0f);
        SetColor(ring1, new Color(0.2f, 0.8f, 1.0f));
        UnityEngine.Object.Destroy(ring1.GetComponent<Collider>());
        ring1.AddComponent<PortalRingRotator>().speed = 45f;

        // Anillo 2 (purpura, gira rapido opuesto)
        GameObject ring2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring2.name = "PortalRing2";
        ring2.transform.SetParent(portalRoot.transform);
        ring2.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        ring2.transform.localScale    = new Vector3(3.8f, 0.06f, 3.8f);
        ring2.transform.localRotation = Quaternion.Euler(80f, 60f, 0f);
        SetColor(ring2, new Color(0.6f, 0.2f, 1.0f));
        UnityEngine.Object.Destroy(ring2.GetComponent<Collider>());
        ring2.AddComponent<PortalRingRotator>().speed = -60f;

        // Esfera central de energia
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "PortalOrb";
        orb.transform.SetParent(portalRoot.transform);
        orb.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        orb.transform.localScale    = Vector3.one * 0.9f;
        SetColor(orb, new Color(0.3f, 0.9f, 1.0f));
        UnityEngine.Object.Destroy(orb.GetComponent<Collider>());

        // Particulas
        CreatePortalParticles(portalRoot, new Color(0.0f, 0.8f, 1.0f));

        // Luz principal
        GameObject lightObj = new GameObject("PortalLight");
        lightObj.transform.SetParent(portalRoot.transform);
        lightObj.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        Light light = lightObj.AddComponent<Light>();
        light.type      = LightType.Point;
        light.range     = 14f;
        light.color     = Color.red;
        light.intensity = 2.5f;

        // Trigger para el jugador
        BoxCollider trigger = portalRoot.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size      = new Vector3(3.6f, 5.0f, 2.5f);
        trigger.center    = new Vector3(0f, 2.5f, 0f);

        portalRoot.AddComponent<ExitPortal>();
        Debug.Log("[GameInitializer] Portal 3D espectacular creado.");
    }

    private static void CreatePortalPillar(GameObject parent, Vector3 localPos)
    {
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "PortalPillar";
        pillar.transform.SetParent(parent.transform);
        pillar.transform.localPosition = localPos;
        pillar.transform.localScale    = new Vector3(0.45f, 2.5f, 0.45f);
        SetColor(pillar, new Color(0.1f, 0.5f, 1.0f));
        UnityEngine.Object.Destroy(pillar.GetComponent<Collider>());

        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cap.name = "PillarCap";
        cap.transform.SetParent(parent.transform);
        cap.transform.localPosition = localPos + Vector3.up * 2.5f;
        cap.transform.localScale    = Vector3.one * 0.55f;
        SetColor(cap, new Color(0.3f, 0.9f, 1.0f));
        UnityEngine.Object.Destroy(cap.GetComponent<Collider>());
    }

    private static void CreatePortalParticles(GameObject parent, Color color)
    {
        GameObject pObj = new GameObject("PortalParticles");
        pObj.transform.SetParent(parent.transform);
        pObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);

        ParticleSystem ps = pObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor    = color;
        main.startSize     = new ParticleSystem.MinMaxCurve(0.1f, 0.35f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(0.5f, 2.5f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 2.5f);
        main.loop          = true;
        main.playOnAwake   = true;
        main.maxParticles  = 80;

        var emission = ps.emission;
        emission.rateOverTime = 30f;

        var shape = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 1.5f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.y = new ParticleSystem.MinMaxCurve(1.0f, 3.0f);
    }

    private static void SetColor(GameObject obj, Color color)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null) rend.material.color = color;
    }

    private static void ConfigureEnemyAI(Vector3 rawPos)
    {
        if (FindFirstObjectByType<EnemyAI>() != null) return;

        Vector3 pos = rawPos;
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up * 8f, Vector3.down, out hit, 20f))
            pos = hit.point + Vector3.up * 1.0f;

        GameObject enemyObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemyObj.name = "Enemy_Guardian";
        enemyObj.transform.position   = pos;
        enemyObj.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

        Renderer rend = enemyObj.GetComponent<Renderer>();
        if (rend != null) rend.material.color = new Color(0.8f, 0.1f, 0.1f);

        NavMeshAgent agent = enemyObj.AddComponent<NavMeshAgent>();
        agent.speed            = 2.6f;
        agent.stoppingDistance = 1.3f;

        enemyObj.AddComponent<EnemyAI>();
    }

    private static void ConfigureGroundHazard()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground != null && ground.GetComponent<KillObstacle>() == null)
            ground.AddComponent<KillObstacle>();
    }
}
