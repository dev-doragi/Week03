using System.Collections.Generic;
using UnityEngine;

public abstract class DungeonPropPlacer : MonoBehaviour
{
    [SerializeField] protected List<PropPlacementSO> _placementSettings = new();
    [SerializeField] protected Transform _spawnRoot;
    [SerializeField] protected Vector3 _spawnOffset = new Vector3(0.5f, 0.5f, 0f);

    protected readonly List<GameObject> _spawnedObjects = new();
    protected readonly Dictionary<PropPlacementSO, int> _placedCountBySetting = new();

    public void PlaceProps(DungeonLayout layout)
    {
        if (layout == null)
            return;

        OnBeforePlace(layout);

        for (int i = 0; i < layout.Rooms.Count; i++)
        {
            PlaceRoomProps(layout, layout.Rooms[i]);
        }

        OnAfterPlace(layout);
    }

    protected virtual void OnBeforePlace(DungeonLayout layout)
    {
        ClearPlacedProps();
        _placedCountBySetting.Clear();

        for (int i = 0; i < _placementSettings.Count; i++)
        {
            PropPlacementSO setting = _placementSettings[i];
            if (setting == null)
                continue;

            _placedCountBySetting[setting] = 0;
        }
    }

    protected virtual void OnAfterPlace(DungeonLayout layout)
    {
    }

    protected abstract void PlaceRoomProps(DungeonLayout layout, DungeonRoom room);

    public void ClearPlacedProps()
    {
        Transform root = _spawnRoot != null ? _spawnRoot : transform;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            GameObject target = root.GetChild(i).gameObject;

#if UNITY_EDITOR
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
#else
            Destroy(target);
#endif
        }

        _spawnedObjects.Clear();
    }

    protected bool HasReachedDungeonLimit(PropPlacementSO setting)
    {
        if (_placedCountBySetting.TryGetValue(setting, out int placedCount) == false)
            return false;

        return placedCount >= setting.MaxPerDungeon;
    }

    protected bool TryGetFootprintTiles(
        DungeonRoom room,
        HashSet<Vector2Int> blockedTiles,
        Vector2Int originPosition,
        Vector2Int footprint,
        PlacementOriginCorner originCorner,
        out List<Vector2Int> footprintTiles)
    {
        footprintTiles = new List<Vector2Int>();

        GetOffsetRange(originCorner, footprint, out int minX, out int maxX, out int minY, out int maxY);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int targetTile = originPosition + new Vector2Int(x, y);

                if (room.FloorTiles.Contains(targetTile) == false)
                    return false;

                if (blockedTiles != null && blockedTiles.Contains(targetTile))
                    return false;

                if (room.OccupiedTiles.Contains(targetTile))
                    return false;

                footprintTiles.Add(targetTile);
            }
        }

        return footprintTiles.Count > 0;
    }

    protected bool HasEnoughSpacing(
        Vector2Int targetPosition,
        List<Vector2Int> placedPositions,
        int minSpacing)
    {
        if (minSpacing <= 0)
            return true;

        for (int i = 0; i < placedPositions.Count; i++)
        {
            if (Vector2Int.Distance(targetPosition, placedPositions[i]) < minSpacing)
                return false;
        }

        return true;
    }

    protected GameObject SpawnProp(GameObject prefab, Vector2Int position)
    {
        if (prefab == null)
            return null;

        Transform parent = _spawnRoot != null ? _spawnRoot : transform;
        Vector3 spawnPosition = new Vector3(position.x, position.y, 0f) + _spawnOffset;

        GameObject spawnedObject = Instantiate(prefab, spawnPosition, Quaternion.identity, parent);
        _spawnedObjects.Add(spawnedObject);
        return spawnedObject;
    }

    protected GameObject SpawnProp(
        GameObject prefab,
        Vector2Int position,
        Quaternion rotation,
        Vector3 additionalOffset)
    {
        if (prefab == null)
            return null;

        Transform parent = _spawnRoot != null ? _spawnRoot : transform;
        Vector3 spawnPosition = new Vector3(position.x, position.y, 0f) + _spawnOffset + additionalOffset;

        GameObject spawnedObject = Instantiate(prefab, spawnPosition, rotation, parent);
        _spawnedObjects.Add(spawnedObject);
        return spawnedObject;
    }

    protected void OccupyTiles(DungeonRoom room, List<Vector2Int> footprintTiles)
    {
        for (int i = 0; i < footprintTiles.Count; i++)
        {
            room.OccupiedTiles.Add(footprintTiles[i]);
        }
    }

    protected void GetOffsetRange(
        PlacementOriginCorner originCorner,
        Vector2Int footprint,
        out int minX,
        out int maxX,
        out int minY,
        out int maxY)
    {
        minX = 0;
        maxX = footprint.x - 1;
        minY = 0;
        maxY = footprint.y - 1;

        switch (originCorner)
        {
            case PlacementOriginCorner.BottomLeft:
                break;

            case PlacementOriginCorner.BottomRight:
                minX = -(footprint.x - 1);
                maxX = 0;
                break;

            case PlacementOriginCorner.TopLeft:
                minY = -(footprint.y - 1);
                maxY = 0;
                break;

            case PlacementOriginCorner.TopRight:
                minX = -(footprint.x - 1);
                maxX = 0;
                minY = -(footprint.y - 1);
                maxY = 0;
                break;
        }
    }

    protected PlacementOriginCorner GetCornerOrigin(DungeonRoom room, Vector2Int cornerTile)
    {
        bool hasUp = room.FloorTiles.Contains(cornerTile + Vector2Int.up);
        bool hasDown = room.FloorTiles.Contains(cornerTile + Vector2Int.down);
        bool hasLeft = room.FloorTiles.Contains(cornerTile + Vector2Int.left);
        bool hasRight = room.FloorTiles.Contains(cornerTile + Vector2Int.right);

        if (hasRight && hasUp)
            return PlacementOriginCorner.BottomLeft;

        if (hasLeft && hasUp)
            return PlacementOriginCorner.BottomRight;

        if (hasRight && hasDown)
            return PlacementOriginCorner.TopLeft;

        return PlacementOriginCorner.TopRight;
    }

    protected Quaternion GetWallRotation(GameObject prefab, Vector2Int direction)
    {
        Quaternion baseRotation = prefab.transform.rotation;

        if (direction == Vector2Int.right)
            return Quaternion.Euler(0f, 180f, 0f) * baseRotation;

        return baseRotation;
    }

    protected Vector3 GetWallAdditionalOffset(Vector2Int direction)
    {
        if (direction == Vector2Int.up)
            return new Vector3(0f, 1f, 0f);

        return Vector3.zero;
    }

    protected void Shuffle<T>(List<T> values)
    {
        for (int i = 0; i < values.Count; i++)
        {
            int randomIndex = Random.Range(i, values.Count);
            (values[i], values[randomIndex]) = (values[randomIndex], values[i]);
        }
    }
}