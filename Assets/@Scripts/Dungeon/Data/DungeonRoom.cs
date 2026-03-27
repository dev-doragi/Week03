using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Normal,
    Start,
    Treasure,
    Store,
    Special,
    Boss
}

public class DungeonRoom
{
    public RoomType RoomType { get; set; }
    public BoundsInt Bounds { get; }
    public Vector2Int Center { get; }
    public HashSet<Vector2Int> FloorTiles { get; }

    public HashSet<Vector2Int> NearWallTilesUp { get; } = new();
    public HashSet<Vector2Int> NearWallTilesDown { get; } = new();
    public HashSet<Vector2Int> NearWallTilesLeft { get; } = new();
    public HashSet<Vector2Int> NearWallTilesRight { get; } = new();
    public HashSet<Vector2Int> CornerTiles { get; } = new();
    public HashSet<Vector2Int> InnerTiles { get; } = new();
    public HashSet<Vector2Int> OccupiedTiles { get; } = new();

    public DungeonRoom(BoundsInt bounds, Vector2Int center, HashSet<Vector2Int> floorTiles)
    {
        Bounds = bounds;
        Center = center;
        FloorTiles = floorTiles ?? new HashSet<Vector2Int>();
    }

    public void ClearAnalysis()
    {
        NearWallTilesUp.Clear();
        NearWallTilesDown.Clear();
        NearWallTilesLeft.Clear();
        NearWallTilesRight.Clear();
        CornerTiles.Clear();
        InnerTiles.Clear();
    }

    public void ClearOccupancy()
    {
        OccupiedTiles.Clear();
    }
}