using System.Collections.Generic;
using UnityEngine;

public enum PlacementMode
{
    Random,
    WallInterval,
    RoomCenter,
    RoomCorner,
    SpecialRoomOnly
}

[CreateAssetMenu(
    fileName = "SO_PropPlacement",
    menuName = "Dungeon/Prop Placement")]
public class PropPlacementSO : ScriptableObject
{
    [SerializeField] private List<GameObject> _prefabs = new();
    [SerializeField] private List<GameObject> _upperWallPrefabs = new();
    [SerializeField] private List<GameObject> _lowerWallPrefabs = new();
    [SerializeField] private List<GameObject> _leftWallPrefabs = new();
    [SerializeField] private List<GameObject> _rightWallPrefabs = new();

    [SerializeField] private Vector2Int _footprint = Vector2Int.one;

    [SerializeField] private PlacementMode _placementMode = PlacementMode.Random;
    [SerializeField] private RoomType[] _allowedRoomTypes;

    [SerializeField][Min(0)] private int _maxPerRoom = 1;
    [SerializeField][Min(0)] private int _maxPerDungeon = 10;
    [SerializeField][Min(0)] private int _minSpacing = 0;
    [SerializeField][Range(0f, 1f)] private float _spawnChance = 1f;

    public IReadOnlyList<GameObject> Prefabs => _prefabs;
    public IReadOnlyList<GameObject> UpperWallPrefabs => _upperWallPrefabs;
    public IReadOnlyList<GameObject> LowerWallPrefabs => _lowerWallPrefabs;
    public IReadOnlyList<GameObject> LeftWallPrefabs => _leftWallPrefabs;
    public IReadOnlyList<GameObject> RightWallPrefabs => _rightWallPrefabs;

    public Vector2Int Footprint => _footprint;
    public PlacementMode PlacementMode => _placementMode;
    public IReadOnlyList<RoomType> AllowedRoomTypes => _allowedRoomTypes;

    public int MaxPerRoom => _maxPerRoom;
    public int MaxPerDungeon => _maxPerDungeon;
    public int MinSpacing => _minSpacing;
    public float SpawnChance => _spawnChance;

    public bool HasValidPrefab()
    {
        return HasValidPrefab(_prefabs);
    }

    public bool HasValidWallPrefab(Vector2Int wallDirection)
    {
        List<GameObject> source = GetWallPrefabList(wallDirection);
        return HasValidPrefab(source);
    }

    public bool IsAllowedRoom(RoomType roomType)
    {
        if (_allowedRoomTypes == null || _allowedRoomTypes.Length == 0)
            return true;

        for (int i = 0; i < _allowedRoomTypes.Length; i++)
        {
            if (_allowedRoomTypes[i] == roomType)
                return true;
        }

        return false;
    }

    public GameObject GetRandomPrefab()
    {
        return GetRandomPrefabFrom(_prefabs);
    }

    public GameObject GetRandomWallPrefab(Vector2Int wallDirection)
    {
        List<GameObject> source = GetWallPrefabList(wallDirection);
        return GetRandomPrefabFrom(source);
    }

    private List<GameObject> GetWallPrefabList(Vector2Int wallDirection)
    {
        if (wallDirection == Vector2Int.up)
            return _upperWallPrefabs;

        if (wallDirection == Vector2Int.down)
            return _lowerWallPrefabs;

        if (wallDirection == Vector2Int.left)
            return _leftWallPrefabs;

        if (wallDirection == Vector2Int.right)
            return _rightWallPrefabs;

        return null;
    }

    private bool HasValidPrefab(List<GameObject> source)
    {
        if (source == null || source.Count == 0)
            return false;

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] != null)
                return true;
        }

        return false;
    }

    private GameObject GetRandomPrefabFrom(List<GameObject> source)
    {
        if (source == null || source.Count == 0)
            return null;

        List<GameObject> validPrefabs = new();

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] != null)
            {
                validPrefabs.Add(source[i]);
            }
        }

        if (validPrefabs.Count == 0)
            return null;

        int randomIndex = Random.Range(0, validPrefabs.Count);
        return validPrefabs[randomIndex];
    }

    private void OnValidate()
    {
        _footprint.x = Mathf.Max(1, _footprint.x);
        _footprint.y = Mathf.Max(1, _footprint.y);

        _maxPerRoom = Mathf.Max(0, _maxPerRoom);
        _maxPerDungeon = Mathf.Max(0, _maxPerDungeon);
        _minSpacing = Mathf.Max(0, _minSpacing);
        _spawnChance = Mathf.Clamp01(_spawnChance);
    }
}