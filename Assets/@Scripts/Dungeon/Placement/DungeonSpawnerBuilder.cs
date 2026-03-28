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

    [Header("Spawner Prefab")]
    [SerializeField] private EnemySpawner _normalRoomSpawnerPrefab;

    [Header("Hierarchy")]
    [SerializeField] private Transform _encounterRoot;

    [Header("Spawn Settings")]
    [SerializeField] private Vector2 _tileCenterOffset = new Vector2(0.5f, 0.5f);
    [SerializeField] private int _fallbackSpawnPointCount = 4;
    [SerializeField] private bool _excludeRoomCenter = true;
    [SerializeField] private int _minDistanceBetweenSpawnPoints = 2;

    private DungeonGenerator _dungeonGenerator;
    private readonly List<GameObject> _spawnedEncounterObjects = new();

    private void Awake()
    {
        _dungeonGenerator = GetComponent<DungeonGenerator>();
    }

    private void OnEnable()
    {
        if (_dungeonGenerator != null)
        {
            _dungeonGenerator.OnDungeonGenerated += HandleDungeonGenerated;
        }
    }

    private void OnDisable()
    {
        if (_dungeonGenerator != null)
        {
            _dungeonGenerator.OnDungeonGenerated -= HandleDungeonGenerated;
        }
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

        if (_normalRoomSpawnerPrefab == null)
            return;

        for (int i = 0; i < layout.Rooms.Count; i++)
        {
            DungeonRoom room = layout.Rooms[i];
            if (room == null)
                continue;

            if (room.RoomType != RoomType.Normal)
                continue;

            CreateNormalRoomEncounter(layout, room);
        }
    }

    private void CreateNormalRoomEncounter(DungeonLayout layout, DungeonRoom room)
    {
        List<Vector2Int> candidates = CollectSpawnCandidates(layout, room);
        if (candidates.Count == 0)
            return;

        Transform parent = _encounterRoot != null ? _encounterRoot : transform;

        GameObject encounterObject = new GameObject($"Encounter_Normal_{room.Center.x}_{room.Center.y}");
        encounterObject.transform.SetParent(parent);
        encounterObject.transform.position = TileToWorld(room.Center);

        _spawnedEncounterObjects.Add(encounterObject);

        EnemySpawner spawner = Instantiate(
            _normalRoomSpawnerPrefab,
            encounterObject.transform.position,
            Quaternion.identity,
            encounterObject.transform);

        List<Transform> spawnPoints = CreateSpawnPoints(spawner, encounterObject.transform, candidates);
        if (spawnPoints.Count == 0)
        {
            Destroy(encounterObject);
            _spawnedEncounterObjects.Remove(encounterObject);
            return;
        }

        spawner.InitializeSpawnPoints(spawnPoints);

        SpawnTrigger trigger = encounterObject.AddComponent<SpawnTrigger>();
        trigger.Initialize(spawner);

        BoxCollider2D triggerCollider = encounterObject.GetComponent<BoxCollider2D>();
        ConfigureTriggerCollider(triggerCollider, room, encounterObject.transform);
    }

    private List<Transform> CreateSpawnPoints(
        EnemySpawner spawner,
        Transform parent,
        List<Vector2Int> candidates)
    {
        List<Transform> spawnPoints = new();

        int targetCount = spawner.GetMaxRequiredSpawnPointCount();
        if (targetCount <= 0)
        {
            targetCount = Mathf.Min(_fallbackSpawnPointCount, candidates.Count);
        }
        else
        {
            targetCount = Mathf.Min(targetCount, candidates.Count);
        }

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

    private bool IsValidSpawnCandidate(
        DungeonLayout layout,
        DungeonRoom room,
        HashSet<Vector2Int> accessibleTiles,
        Vector2Int tile)
    {
        if (!room.InnerTiles.Contains(tile))
            return false;

        if (!accessibleTiles.Contains(tile))
            return false;

        if (layout.CorridorTiles.Contains(tile))
            return false;

        if (room.OccupiedTiles.Contains(tile))
            return false;

        if (_excludeRoomCenter && tile == room.Center)
            return false;

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

                if (visited.Contains(next))
                    continue;

                if (!room.FloorTiles.Contains(next))
                    continue;

                if (room.OccupiedTiles.Contains(next))
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

    private void ConfigureTriggerCollider(
        BoxCollider2D triggerCollider,
        DungeonRoom room,
        Transform triggerTransform)
    {
        bool hasTile = false;
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (Vector2Int tile in room.FloorTiles)
        {
            hasTile = true;

            if (tile.x < minX) minX = tile.x;
            if (tile.y < minY) minY = tile.y;
            if (tile.x > maxX) maxX = tile.x;
            if (tile.y > maxY) maxY = tile.y;
        }

        if (!hasTile)
            return;

        Vector2 min = new Vector2(minX, minY);
        Vector2 max = new Vector2(maxX + 1f, maxY + 1f);

        Vector2 worldCenter = (min + max) * 0.5f;
        Vector2 size = max - min;

        triggerCollider.isTrigger = true;
        triggerCollider.offset = worldCenter - (Vector2)triggerTransform.position;
        triggerCollider.size = size;
    }

    private Vector3 TileToWorld(Vector2Int tilePosition)
    {
        return (Vector2)tilePosition + _tileCenterOffset;
    }

    private void ClearEncounters()
    {
        for (int i = _spawnedEncounterObjects.Count - 1; i >= 0; i--)
        {
            GameObject spawnedObject = _spawnedEncounterObjects[i];
            if (spawnedObject == null)
                continue;

            Destroy(spawnedObject);
        }

        _spawnedEncounterObjects.Clear();
    }

    private void Shuffle<T>(List<T> values)
    {
        for (int i = 0; i < values.Count; i++)
        {
            int randomIndex = Random.Range(i, values.Count);
            (values[i], values[randomIndex]) = (values[randomIndex], values[i]);
        }
    }
}