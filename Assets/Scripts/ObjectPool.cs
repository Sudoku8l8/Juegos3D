using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de Object Pooling para optimización de memoria y rendimiento.
/// Evita la sobrecarga del Garbage Collector (GC) mediante la reutilización de GameObjects
/// (como efectos de partículas, proyectiles y sonidos) en lugar de Instantiate() y Destroy().
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class PoolItem
    {
        public string tag;
        public GameObject prefab;
        public int size = 10;
        public bool canGrow = true;
    }

    [Header("Configuración de Pools")]
    [SerializeField] private List<PoolItem> pools = new List<PoolItem>();

    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, PoolItem> poolSettings;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolSettings = new Dictionary<string, PoolItem>();

        foreach (PoolItem item in pools)
        {
            if (item.prefab == null) continue;

            Queue<GameObject> objectQueue = new Queue<GameObject>();
            poolSettings[item.tag] = item;

            for (int i = 0; i < item.size; i++)
            {
                GameObject obj = CreateNewObject(item.prefab);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(item.tag, objectQueue);
        }
    }

    private GameObject CreateNewObject(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        return obj;
    }

    /// <summary>
    /// Obtiene un objeto reutilizable del Pool en la posición y rotación indicadas.
    /// </summary>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (poolDictionary == null || !poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPool] La etiqueta '{tag}' no existe en el pool.");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[tag];
        GameObject objectToSpawn = null;

        // Si hay objetos en cola, tomamos el siguiente disponible
        if (queue.Count > 0)
        {
            objectToSpawn = queue.Dequeue();
        }
        else if (poolSettings[tag].canGrow)
        {
            // Si la cola está vacía pero se permite crecer, creamos uno nuevo
            objectToSpawn = CreateNewObject(poolSettings[tag].prefab);
        }

        if (objectToSpawn != null)
        {
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);

            // Reencolamos para futuro uso
            queue.Enqueue(objectToSpawn);
        }

        return objectToSpawn;
    }

    /// <summary>
    /// Devuelve un objeto al pool desactivándolo.
    /// </summary>
    public void ReturnToPool(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(false);
            obj.transform.SetParent(transform);
        }
    }
}
