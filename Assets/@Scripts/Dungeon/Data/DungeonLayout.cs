using System.Collections.Generic;
using UnityEngine;

public class DungeonLayout
{
    public Vector2Int StartPosition { get; }
    public HashSet<Vector2Int> FloorTiles { get; }
    public HashSet<Vector2Int> CorridorTiles { get; }
    public List<DungeonRoom> Rooms { get; }

    public DungeonLayout(
        Vector2Int startPosition,
        HashSet<Vector2Int> floorTiles,
        HashSet<Vector2Int> corridorTiles,
        List<DungeonRoom> rooms)
    {
        StartPosition = startPosition;
        FloorTiles = floorTiles ?? new HashSet<Vector2Int>();
        CorridorTiles = corridorTiles ?? new HashSet<Vector2Int>();
        Rooms = rooms ?? new List<DungeonRoom>();
    }

    public void ClearOccupancy()
    {
        for (int i = 0; i < Rooms.Count; i++)
        {
            Rooms[i].ClearOccupancy();
        }
    }
}