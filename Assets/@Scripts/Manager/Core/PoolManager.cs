using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PoolManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    [Header("Pooling")]
    [Tooltip("0 = no automatic prewarm. If >0, CreatePool will pre-create this many instances when called without explicit size.")]
    [SerializeField] private int _defaultPrewarm = 0;
    public int DefaultPrewarm => _defaultPrewarm;

    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
    private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();
    private readonly Dictionary<GameObject, Transform> _poolContainers = new();
    private readonly HashSet<GameObject> _activeInstances = new();

    public void Initialize()
    {
        if (IsInitialized) return;
        IsInitialized = true;
    }

    public void CreatePool(GameObject prefab, int initialSize)
    {
        if (prefab == null || initialSize <= 0)
            return;

        Queue<GameObject> pool = GetOrCreatePool(prefab);

        if (pool.Count >= initialSize)
            return;

        int toCreate = initialSize - pool.Count;
        for (int i = 0; i < toCreate; i++)
        {
            GameObject instance = CreateNewInstance(prefab);
            ReturnInternal(prefab, instance);
        }
    }

    public void CreatePool(GameObject prefab)
    {
        CreatePool(prefab, DefaultPrewarm);
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("[PoolManager] Prefab is null.");
            return null;
        }

        Queue<GameObject> pool = GetOrCreatePool(prefab);

        GameObject instance;

        while (pool.Count > 0)
        {
            instance = pool.Dequeue();

            if (instance != null)
            {
                instance.transform.SetPositionAndRotation(position, rotation);
                instance.SetActive(true);
                _activeInstances.Add(instance);
                return instance;
            }
        }

        instance = CreateNewInstance(prefab);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        _activeInstances.Add(instance);

        return instance;
    }

    public void Return(GameObject instance)
    {
        if (instance == null)
            return;

        if (!_instanceToPrefab.TryGetValue(instance, out GameObject prefab))
        {
            Debug.LogWarning("[PoolManager] Returned object was not created by PoolManager. Destroying it.");
            Destroy(instance);
            return;
        }

        ReturnInternal(prefab, instance);
    }

    private GameObject CreateNewInstance(GameObject prefab)
    {
        Transform container = GetOrCreateContainer(prefab);
        GameObject instance = Instantiate(prefab, container);

        instance.SetActive(false);

        _instanceToPrefab[instance] = prefab;
        return instance;
    }

    private void ReturnInternal(GameObject prefab, GameObject instance)
    {
        if (instance == null)
            return;

        _activeInstances.Remove(instance);

        DOTween.Kill(instance.transform, complete: false);

        instance.transform.SetParent(GetOrCreateContainer(prefab), false);
        instance.SetActive(false);

        Queue<GameObject> pool = GetOrCreatePool(prefab);
        pool.Enqueue(instance);
    }

    private Queue<GameObject> GetOrCreatePool(GameObject prefab)
    {
        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = new Queue<GameObject>();
            _pools[prefab] = pool;
        }
        return pool;
    }

    private Transform GetOrCreateContainer(GameObject prefab)
    {
        if (prefab == null) return this.transform;

        if (_poolContainers.TryGetValue(prefab, out var existing) && existing != null)
            return existing;

        var containerGo = new GameObject($"Pool-{prefab.name}");
        containerGo.transform.SetParent(this.transform, false);

        var container = containerGo.transform;
        _poolContainers[prefab] = container;
        return container;
    }

    public void ClearRuntimeObjects()
    {
        if (_activeInstances.Count == 0)
            return;

        List<GameObject> runtimeObjects = new(_activeInstances);

        for (int i = 0; i < runtimeObjects.Count; i++)
        {
            GameObject instance = runtimeObjects[i];
            if (instance == null)
                continue;

            Return(instance);
        }

        _activeInstances.Clear();
    }
}