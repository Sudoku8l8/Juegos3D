using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

/// <summary>
/// Herramienta de Editor para configurar en 1 clic todos los elementos solicitados
/// por la consigna del Capstone (Managers, Player, Cristales, Enemigo con IA, Portal y UI).
/// Accesible desde el menú superior de Unity: Tools -> Capstone 3D.
/// </summary>
public class CapstoneSceneSetup : EditorWindow
{
    [MenuItem("Tools/Capstone 3D/Configurar Escena Automáticamente")]
    [MenuItem("Window/Capstone 3D/Configurar Escena Automáticamente")]
    [MenuItem("GameObject/Capstone 3D/Configurar Escena Automáticamente", false, 0)]
    public static void SetupFullScene()
    {
        Debug.Log("==== [CAPSTONE 3D] INICIANDO CONFIGURACIÓN PROFESIONAL ====");

        SetupGameSystems();
        SetupPlayer();
        SetupCrystals();
        SetupExitPortal();
        SetupEnemyGuardian();

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("==== [CAPSTONE 3D] ¡CONFIGURACIÓN COMPLETADA CON ÉXITO! Guarda tu escena con Ctrl+S ====");
        EditorUtility.DisplayDialog("Capstone 3D", "¡La escena ha sido configurada con éxito!\n\n- Game Systems (GameManager, Audio, UI, ObjectPool)\n- Player con Vidas y Salud\n- Cristales Coleccionables\n- Portal de Escape (Condición de Victoria)\n- Enemigo Guardián con IA (Patrulla y Persecución)", "Entendido");
    }

    [MenuItem("Window/Capstone 3D/Abrir Panel de Control")]
    public static void ShowWindow()
    {
        GetWindow<CapstoneSceneSetup>("Panel Capstone 3D");
    }

    private void OnGUI()
    {
        GUILayout.Space(15);
        GUILayout.Label("Proyecto Capstone: Videojuego 3D Interactivo", EditorStyles.boldLabel);
        GUILayout.Label("Configurador automático de Escena y Sistemas", EditorStyles.miniLabel);

        GUILayout.Space(20);
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
        if (GUILayout.Button("⚡ Configurar Escena Completa (1 Clic)", GUILayout.Height(50)))
        {
            SetupFullScene();
        }

        GUI.backgroundColor = Color.white;
        GUILayout.Space(15);
        EditorGUILayout.HelpBox("Este botón crea y conecta automáticamente:\n• 5 Cristales de Energía con efectos y audio\n• Enemigo Guardián con IA (Patrulla y Persecución)\n• Portal de Escape (Condición de Victoria)\n• Vidas, Salud e Invulnerabilidad en el Player\n• GameManager, UIManager y ObjectPool", MessageType.Info);
    }

    private static void SetupGameSystems()
    {
        GameObject systems = GameObject.Find("[GAME_SYSTEMS]");
        if (systems == null)
        {
            systems = new GameObject("[GAME_SYSTEMS]");
            Undo.RegisterCreatedObjectUndo(systems, "Create Game Systems");
        }

        if (systems.GetComponent<GameManager>() == null) systems.AddComponent<GameManager>();
        if (systems.GetComponent<AudioManager>() == null) systems.AddComponent<AudioManager>();
        if (systems.GetComponent<UIManager>() == null) systems.AddComponent<UIManager>();
        if (systems.GetComponent<ObjectPool>() == null) systems.AddComponent<ObjectPool>();
    }

    private static void SetupPlayer()
    {
        playerController pc = Object.FindFirstObjectByType<playerController>();
        if (pc != null)
        {
            GameObject playerObj = pc.gameObject;
            if (!playerObj.CompareTag("Player"))
            {
                playerObj.tag = "Player";
            }

            // Eliminar la cápsula placeholder desactivada para que solo se vea el modelo del niño
            MeshRenderer rootMesh = playerObj.GetComponent<MeshRenderer>();
            if (rootMesh != null && !rootMesh.enabled)
            {
                Undo.DestroyObjectImmediate(rootMesh);
                MeshFilter mf = playerObj.GetComponent<MeshFilter>();
                if (mf != null) Undo.DestroyObjectImmediate(mf);
            }

            if (playerObj.GetComponent<PlayerHealth>() == null)
            {
                Undo.AddComponent<PlayerHealth>(playerObj);
            }
        }
    }

    private static void SetupCrystals()
    {
        // Buscar prefab de cristal en el proyecto
        string[] guids = AssetDatabase.FindAssets("PFB_VD_Collectible_Crystal_01 t:Prefab");
        GameObject crystalPrefab = null;
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            crystalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        // Buscar checkpoints o plataformas para situar 5 cristales
        CheckPoint[] checkpoints = Object.FindObjectsByType<CheckPoint>(FindObjectsSortMode.None);
        int spawned = 0;

        // Contenedor organizado
        GameObject crystalGroup = GameObject.Find("Crystals_Group");
        if (crystalGroup == null)
        {
            crystalGroup = new GameObject("Crystals_Group");
            Undo.RegisterCreatedObjectUndo(crystalGroup, "Create Crystals Group");
        }

        // Evitar duplicados si ya existen
        if (crystalGroup.transform.childCount >= 5) return;

        playerController pc = Object.FindFirstObjectByType<playerController>();
        Vector3 basePos = pc != null ? pc.transform.position : Vector3.zero;

        Vector3[] platformPositions = new Vector3[]
        {
            new Vector3(-1.8f, 14.5f, 15.5f),   // Cristal 1: Plataforma inicial (Zona 1)
            new Vector3(-1.8f, 14.5f, 24.6f),   // Cristal 2: Segunda plataforma (Zona 1)
            new Vector3(-8.3f, 9.5f, 32.8f),    // Cristal 3: Plataforma intermedia (Zona 2)
            new Vector3(-19.5f, 12.0f, 34.4f),  // Cristal 4: Plataforma elevada (Zona 2)
            new Vector3(-36.5f, 3.2f, 32.5f)    // Cristal 5: Plataforma final (Zona 3)
        };

        for (int i = 0; i < 5; i++)
        {
            Vector3 pos = platformPositions[i];

            GameObject crystalInstance;
            if (crystalPrefab != null)
            {
                crystalInstance = (GameObject)PrefabUtility.InstantiatePrefab(crystalPrefab, crystalGroup.transform);
                crystalInstance.transform.position = pos;
            }
            else
            {
                crystalInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crystalInstance.transform.SetParent(crystalGroup.transform);
                crystalInstance.transform.position = pos;
                crystalInstance.transform.localScale = Vector3.one * 0.7f;
                Renderer rend = crystalInstance.GetComponent<Renderer>();
                if (rend != null) rend.sharedMaterial.color = Color.cyan;
            }

            crystalInstance.name = $"Crystal_Collectible_{i + 1}";
            if (crystalInstance.GetComponent<CollectibleItem>() == null)
            {
                crystalInstance.AddComponent<CollectibleItem>();
            }

            Collider col = crystalInstance.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            // Luz de brillo suave
            GameObject glow = new GameObject("CrystalGlowLight");
            glow.transform.SetParent(crystalInstance.transform);
            glow.transform.localPosition = Vector3.zero;
            Light l = glow.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = Color.cyan;
            l.range = 5.0f;
            l.intensity = 2.0f;

            Undo.RegisterCreatedObjectUndo(crystalInstance, "Create Crystal Instance");
        }
    }

    private static void SetupExitPortal()
    {
        if (Object.FindFirstObjectByType<ExitPortal>() != null) return;

        Vector3 portalPos = new Vector3(-36.26f, 2.8f, 42.35f);

        GameObject portalObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        portalObj.name = "ExitPortal_Goal";
        portalObj.transform.position = portalPos;
        portalObj.transform.localScale = new Vector3(3f, 0.2f, 3f);

        Collider col = portalObj.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        portalObj.AddComponent<ExitPortal>();

        // Luz de ambientación
        GameObject lightObj = new GameObject("PortalLight");
        lightObj.transform.SetParent(portalObj.transform);
        lightObj.transform.localPosition = Vector3.up * 2f;
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 10f;
        light.color = Color.cyan;

        Undo.RegisterCreatedObjectUndo(portalObj, "Create Exit Portal");
    }

    private static void SetupEnemyGuardian()
    {
        if (Object.FindFirstObjectByType<EnemyAI>() != null) return;

        Vector3 basePos = new Vector3(-14.0f, 10.2f, 28.0f);

        GameObject enemyObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemyObj.name = "Enemy_Guardian";
        enemyObj.transform.position = basePos;
        enemyObj.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

        Renderer rend = enemyObj.GetComponent<Renderer>();
        if (rend != null) rend.sharedMaterial.color = Color.red;

        NavMeshAgent agent = enemyObj.AddComponent<NavMeshAgent>();
        agent.speed = 2.8f;
        agent.stoppingDistance = 1.3f;

        EnemyAI ai = enemyObj.AddComponent<EnemyAI>();

        // Crear 2 Waypoints para la patrulla
        GameObject waypointsParent = new GameObject("Enemy_Waypoints");
        waypointsParent.transform.position = basePos;

        GameObject wp1 = new GameObject("Waypoint_1");
        wp1.transform.SetParent(waypointsParent.transform);
        wp1.transform.position = basePos + new Vector3(-5f, 0f, 2f);

        GameObject wp2 = new GameObject("Waypoint_2");
        wp2.transform.SetParent(waypointsParent.transform);
        wp2.transform.position = basePos + new Vector3(5f, 0f, -2f);

        SerializedObject so = new SerializedObject(ai);
        SerializedProperty wpProp = so.FindProperty("waypoints");
        if (wpProp != null)
        {
            wpProp.arraySize = 2;
            wpProp.GetArrayElementAtIndex(0).objectReferenceValue = wp1.transform;
            wpProp.GetArrayElementAtIndex(1).objectReferenceValue = wp2.transform;
            so.ApplyModifiedProperties();
        }

        Undo.RegisterCreatedObjectUndo(enemyObj, "Create Enemy Guardian");
        Undo.RegisterCreatedObjectUndo(waypointsParent, "Create Enemy Waypoints");

        // Asegurar que caer al piso rojo dañe y reaparezca al jugador
        GameObject ground = GameObject.Find("Ground");
        if (ground != null && ground.GetComponent<KillObstacle>() == null)
        {
            Undo.AddComponent<KillObstacle>(ground);
        }
    }
}
