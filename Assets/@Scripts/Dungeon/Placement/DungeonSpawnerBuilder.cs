using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSpawnerBuilder : MonoBehaviour
{
    private static readonly Vector2Int[] CARDINAL_DIRECTIONS =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    [Header("Spawner Prefabs")]
    [SerializeField] private List<EnemySpawner> _normalRoomSpawnerPrefabs = new();
    [SerializeField] private EnemySpawner _bossRoomSpawnerPrefab;
    [SerializeField] private string _spawnTriggerLayerName = "Spawner";
    [SerializeField] private float _triggerInset = 0.5f;

    [SerializeField] private GameObject _doorBlockPrefab;

    [Header("Hierarchy")]
    [SerializeField] private Transform _encounterRoot;

    [Header("Spawn Settings")]
    [SerializeField] private Vector2 _tileCenterOffset = new Vector2(0.5f, 0.5f);
    [SerializeField] private int _fallbackSpawnPointCount = 4;
    [SerializeField] private bool _excludeRoomCenter = true;
    [SerializeField] private int _minDistanceBetweenSpawnPoints = 2;

    private DungeonGenerator _dungeonGenerator;
    private readonly List<GameObject> _spawnedEncounterObjects = new();
    private Transform _spawnTarget;
    private readonly List<EnemySpawner> _spawnedSpawners = new();

    public event Action OnBossRoomCleared;

    private void Awake()
    {
        _dungeonGenerator = GetComponent<DungeonGenerator>();
    }

    private void OnEnable()
    {
        if (_dungeonGenerator != null)
            _dungeonGenerator.OnDungeonGenerated += HandleDungeonGenerated;
    }

    private void OnDisable()
    {
        if (_dungeonGenerator != null)
            _dungeonGenerator.OnDungeonGenerated -= HandleDungeonGenerated;

        UnsubscribeAllSpawners();
    }

    private void HandleDungeonGenerated(DungeonLayout layout)
    {
        Build(layout);
    }

    public void Build(DungeonLayout layout)
    {
        ClearEncounters();

        if (layout == null)
            return;

        for (int i = 0; i < layout.Rooms.Count; i++)
        {
            DungeonRoom room = layout.Rooms[i];
            if (room == null)
                continue;

            if (room.RoomType == RoomType.Normal)
            {
                EnemySpawner normalSpawnerPrefab = GetRandomNormalSpawnerPrefab();
                if (normalSpawnerPrefab != null)
                    CreateRoomEncounter(layout, room, normalSpawnerPrefab, "Normal");
            }
            else if (room.RoomType == RoomType.Boss)
            {
                if (_bossRoomSpawnerPrefab != null)
                    CreateRoomEncounter(layout, room, _bossRoomSpawnerPrefab, "Boss");
            }
        }
    }

    public void SetSpawnTarget(Transform spawnTarget)
    {
        _spawnTarget = spawnTarget;

        for (int i = 0; i < _spawnedSpawners.Count; i++)
        {
            if (_spawnedSpawners[i] != null)
                _spawnedSpawners[i].SetTarget(_spawnTarget);
        }
    }

    private EnemySpawner GetRandomNormalSpawnerPrefab()
    {
        if (_normalRoomSpawnerPrefabs == null || _normalRoomSpawnerPrefabs.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, _normalRoomSpawnerPrefabs.Count);
        return _normalRoomSpawnerPrefabs[randomIndex];
    }

    private void CreateRoomEncounter(DungeonLayout layout, DungeonRoom room, EnemySpawner selectedPrefab, string encounterLabel)
    {
        List<Vector2Int> candidates = CollectSpawnCandidates(layout, room);
        if (candidates.Count == 0)
            return;

        Transform parent = _encounterRoot != null ? _encounterRoot : transform;

        GameObject encounterObject = new GameObject($"Encounter_{encounterLabel}_{room.Center.x}_{room.Center.y}");
        encounterObject.transform.SetParent(parent);
        encounterObject.transform.position = TileToWorld(room.Center);

        _spawnedEncounterObjects.Add(encounterObject);

        EnemySpawner spawner = Instantiate(
            selectedPrefab,
            encounterObject.transform.position,
            Quaternion.identity,
            encounterObject.transform);

        _spawnedSpawners.Add(spawner);

        if (room.RoomType == RoomType.Boss)
        {
            spawner.OnBossEncounterCleared -= HandleBossEncounterCleared;
            spawner.OnBossEncounterCleared += HandleBossEncounterCleared;
        }

        if (_spawnTarget != null)
            spawner.SetTarget(_spawnTarget);

        List<GameObject> doorBlocks = CreateDoorBlocks(layout, room, encounterObject.transform);
        spawner.SetBlocks(doorBlocks.ToArray());

        List<Transform> spawnPoints = CreateSpawnPoints(spawner, encounterObject.transform, candidates);
        if (spawnPoints.Count == 0)
        {
            if (room.RoomType == RoomType.Boss)
                spawner.OnBossEncounterCleared -= HandleBossEncounterCleared;

            _spawnedSpawners.Remove(spawner);
            Destroy(encounterObject);
            _spawnedEncounterObjects.Remove(encounterObject);
            return;
        }

        spawner.InitializeSpawnPoints(spawnPoints);

        BoxCollider2D trigger = encounterObject.AddComponent<BoxCollider2D>();
        int triggerLayer = LayerMask.NameToLayer(_spawnTriggerLayerName);
        if (triggerLayer != -1)
            encounterObject.layer = triggerLayer;

        trigger.isTrigger = true;

        BoundsInt bounds = room.Bounds;
        float width = Mathf.Max(0.1f, bounds.size.x - (_triggerInset * 2f));
        float height = Mathf.Max(0.1f, bounds.size.y - (_triggerInset * 2f));
        trigger.size = new Vector2(width, height);
        trigger.offset = Vector2.zero;

        SpawnTrigger spawnTrigger = encounterObject.AddComponent<SpawnTrigger>();
        spawnTrigger.Initialize(spawner);
    }

    private void HandleBossEncounterCleared(EnemySpawner spawner)
    {
        OnBossRoomCleared?.Invoke();
    }

    private List<Transform> CreateSpawnPoints(EnemySpawner spawner, Transform parent, List<Vector2Int> candidates)
    {
        List<Transform> spawnPoints = new();

        int targetCount = spawner.GetMaxRequiredSpawnPointCount();
        if (targetCount <= 0)
            targetCount = Mathf.Min(_fallbackSpawnPointCount, candidates.Count);
        else
            targetCount = Mathf.Min(targetCount, candidates.Count);

        for (int i = 0; i < targetCount; i++)
        {
            GameObject spawnPointObject = new GameObject($"SpawnPoint_{i:00}");
            spawnPointObject.transform.SetParent(parent);
            spawnPointObject.transform.position = TileToWorld(candidates[i]);
            spawnPoints.Add(spawnPointObject.transform);
        }

        return spawnPoints;
    }

    private List<Vector2Int> CollectSpawnCandidates(DungeonLayout layout, DungeonRoom room)
    {
        HashSet<Vector2Int> accessibleTiles = FindAccessibleTiles(layout, room);
        List<Vector2Int> filteredCandidates = new();

        foreach (Vector2Int tile in room.InnerTiles)
        {
            if (!IsValidSpawnCandidate(layout, room, accessibleTiles, tile))
                continue;

            filteredCandidates.Add(tile);
        }

        Shuffle(filteredCandidates);

        List<Vector2Int> selectedCandidates = new();

        for (int i = 0; i < filteredCandidates.Count; i++)
        {
            Vector2Int candidate = filteredCandidates[i];

            if (!IsFarEnough(selectedCandidates, candidate, _minDistanceBetweenSpawnPoints))
                continue;

            selectedCandidates.Add(candidate);
        }

        return selectedCandidates;
    }

    private bool IsValidSpawnCandidate(DungeonLayout layout, DungeonRoom room, HashSet<Vector2Int> accessibleTiles, Vector2Int tile)
    {
        if (!room.InnerTiles.Contains(tile)) return false;
        if (!accessibleTiles.Contains(tile)) return false;
        if (layout.CorridorTiles.Contains(tile)) return false;
        if (room.OccupiedTiles.Contains(tile)) return false;
        if (_excludeRoomCenter && tile == room.Center) return false;

        return true;
    }

    private HashSet<Vector2Int> FindAccessibleTiles(DungeonLayout layout, DungeonRoom room)
    {
        HashSet<Vector2Int> entranceTiles = new();

        foreach (Vector2Int floorTile in room.FloorTiles)
        {
            for (int i = 0; i < CARDINAL_DIRECTIONS.Length; i++)
            {
                Vector2Int neighbour = floorTile + CARDINAL_DIRECTIONS[i];
                if (!layout.CorridorTiles.Contains(neighbour))
                    continue;

                entranceTiles.Add(floorTile);
                break;
            }
        }

        if (entranceTiles.Count == 0)
        {
            foreach (Vector2Int tile in room.InnerTiles)
            {
                if (room.OccupiedTiles.Contains(tile))
                    continue;

                entranceTiles.Add(tile);
                break;
            }
        }

        Queue<Vector2Int> frontier = new();
        HashSet<Vector2Int> visited = new();

        foreach (Vector2Int entranceTile in entranceTiles)
        {
            if (room.OccupiedTiles.Contains(entranceTile))
                continue;

            frontier.Enqueue(entranceTile);
            visited.Add(entranceTile);
        }

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            for (int i = 0; i < CARDINAL_DIRECTIONS.Length; i++)
            {
                Vector2Int next = current + CARDINAL_DIRECTIONS[i];
                if (visited.Contains(next) || !room.FloorTiles.Contains(next) || room.OccupiedTiles.Contains(next))
                    continue;

                visited.Add(next);
                frontier.Enqueue(next);
            }
        }

        return visited;
    }

    private bool IsFarEnough(List<Vector2Int> selected, Vector2Int candidate, int minDistance)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            int distance = Mathf.Abs(selected[i].x - candidate.x) + Mathf.Abs(selected[i].y - candidate.y);
            if (distance < minDistance)
                return false;
        }

        return true;
    }

    private Vector3 TileToWorld(Vector2Int tilePosition)
    {
        return (Vector2)tilePosition + _tileCenterOffset;
    }

    private void ClearEncounters()
    {
        UnsubscribeAllSpawners();

        for (int i = _spawnedEncounterObjects.Count - 1; i >= 0; i--)
        {
            if (_spawnedEncounterObjects[i] != null)
                Destroy(_spawnedEncounterObjects[i]);
        }

        _spawnedEncounterObjects.Clear();
        _spawnedSpawners.Clear();
    }

    private void UnsubscribeAllSpawners()
    {
        for (int i = 0; i < _spawnedSpawners.Count; i++)
        {
            if (_spawnedSpawners[i] != null)
                _spawnedSpawners[i].OnBossEncounterCleared -= HandleBossEncounterCleared;
        }
    }

    private void Shuffle<T>(List<T> values)
    {
        for (int i = 0; i < values.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, values.Count);
            (values[i], values[randomIndex]) = (values[randomIndex], values[i]);
        }
    }

    private List<GameObject> CreateDoorBlocks(DungeonLayout layout, DungeonRoom room, Transform parent)
    {
        List<GameObject> blocks = new();
        if (_doorBlockPrefab == null)
            return blocks;

        HashSet<Vector2Int> doorwayTiles = new();
        HashSet<Vector2Int> edgeTiles = new();
        edgeTiles.UnionWith(room.NearWallTilesUp);
        edgeTiles.UnionWith(room.NearWallTilesDown);
        edgeTiles.UnionWith(room.NearWallTilesLeft);
        edgeTiles.UnionWith(room.NearWallTilesRight);
        edgeTiles.UnionWith(room.CornerTiles);

        foreach (Vector2Int edgeTile in edgeTiles)
        {
            for (int i = 0; i < CARDINAL_DIRECTIONS.Length; i++)
            {
                Vector2Int outsideTile = edgeTile + CARDINAL_DIRECTIONS[i];
                if (layout.CorridorTiles.Contains(outsideTile) && !room.FloorTiles.Contains(outsideTile))
                    doorwayTiles.Add(outsideTile);
            }
        }

        foreach (Vector2Int doorTile in doorwayTiles)
        {
            GameObject block = Instantiate(_doorBlockPrefab, TileToWorld(doorTile), Quaternion.identity, parent);
            block.SetActive(false);
            blocks.Add(block);
        }

        return blocks;
    }
}