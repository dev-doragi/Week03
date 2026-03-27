using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RoomAssignRule", menuName = "Dungeon/Room Assigner/Assign Rule")]
public class BasicRoomAssignerSO : RoomAssignerSOBase
{
    [SerializeField] private RoomType _targetRoomType = RoomType.Normal;
    [SerializeField] private RoomSelectPolicy _selectPolicy = RoomSelectPolicy.Largest;
    [SerializeField] private Vector2Int _minRoomSize = new Vector2Int(1, 1);
    [SerializeField] private Vector2Int _maxRoomSize = new Vector2Int(999, 999);
    [SerializeField] private bool _excludeStartRoom = true;
    [SerializeField] private bool _onlyNormalRoom = true;

    public override void AssignRoomTypes(List<DungeonRoom> rooms)
    {
        if (rooms == null || rooms.Count == 0)
            return;

        DungeonRoom targetRoom = FindTargetRoom(rooms);
        if (targetRoom == null)
            return;

        targetRoom.RoomType = _targetRoomType;
    }

    private DungeonRoom FindTargetRoom(List<DungeonRoom> rooms)
    {
        List<DungeonRoom> candidates = new List<DungeonRoom>();

        for (int i = 0; i < rooms.Count; i++)
        {
            DungeonRoom room = rooms[i];

            if (_onlyNormalRoom && room.RoomType != RoomType.Normal)
                continue;

            if (_excludeStartRoom && room.RoomType == RoomType.Start)
                continue;

            int width = room.Bounds.size.x;
            int height = room.Bounds.size.y;

            if (width < _minRoomSize.x || height < _minRoomSize.y)
                continue;

            if (width > _maxRoomSize.x || height > _maxRoomSize.y)
                continue;

            candidates.Add(room);
        }

        if (candidates.Count == 0)
            return null;

        switch (_selectPolicy)
        {
            case RoomSelectPolicy.Largest:
                return FindLargestRoom(candidates);

            case RoomSelectPolicy.Smallest:
                return FindSmallestRoom(candidates);

            case RoomSelectPolicy.Random:
                return FindRandomRoom(candidates);

            case RoomSelectPolicy.RandomSmall:
                return FindRandomSmallRoom(candidates);

            case RoomSelectPolicy.FarthestFromStart:
                return FindFarthestRoom(candidates, rooms);

            case RoomSelectPolicy.FarthestThenLargest:
                return FindFarthestThenLargestRoom(candidates, rooms);
        }

        return null;
    }

    private DungeonRoom FindLargestRoom(List<DungeonRoom> rooms)
    {
        DungeonRoom bestRoom = null;
        int bestScore = int.MinValue;

        for (int i = 0; i < rooms.Count; i++)
        {
            DungeonRoom room = rooms[i];
            int score = room.Bounds.size.x * room.Bounds.size.y;

            if (score > bestScore)
            {
                bestScore = score;
                bestRoom = room;
            }
        }

        return bestRoom;
    }

    private DungeonRoom FindSmallestRoom(List<DungeonRoom> rooms)
    {
        DungeonRoom bestRoom = null;
        int bestScore = int.MaxValue;

        for (int i = 0; i < rooms.Count; i++)
        {
            DungeonRoom room = rooms[i];
            int score = room.Bounds.size.x * room.Bounds.size.y;

            if (score < bestScore)
            {
                bestScore = score;
                bestRoom = room;
            }
        }

        return bestRoom;
    }

    private DungeonRoom FindRandomRoom(List<DungeonRoom> rooms)
    {
        int index = Random.Range(0, rooms.Count);
        return rooms[index];
    }

    private DungeonRoom FindRandomSmallRoom(List<DungeonRoom> rooms)
    {
        List<DungeonRoom> sortedRooms = new List<DungeonRoom>(rooms);
        sortedRooms.Sort((a, b) =>
        {
            int areaA = a.Bounds.size.x * a.Bounds.size.y;
            int areaB = b.Bounds.size.x * b.Bounds.size.y;
            return areaA.CompareTo(areaB);
        });

        int takeCount = Mathf.Max(1, Mathf.CeilToInt(sortedRooms.Count * 0.4f));
        int index = Random.Range(0, takeCount);
        return sortedRooms[index];
    }

    private DungeonRoom FindFarthestRoom(List<DungeonRoom> candidates, List<DungeonRoom> allRooms)
    {
        DungeonRoom startRoom = FindStartRoom(allRooms);
        if (startRoom == null)
            return FindRandomRoom(candidates);

        DungeonRoom bestRoom = null;
        float bestDistance = float.MinValue;

        Vector2 startCenter = startRoom.Bounds.center;

        for (int i = 0; i < candidates.Count; i++)
        {
            DungeonRoom room = candidates[i];
            float distance = Vector2.Distance(startCenter, room.Bounds.center);

            if (distance > bestDistance)
            {
                bestDistance = distance;
                bestRoom = room;
            }
        }

        return bestRoom;
    }

    private DungeonRoom FindFarthestThenLargestRoom(List<DungeonRoom> candidates, List<DungeonRoom> allRooms)
    {
        DungeonRoom startRoom = FindStartRoom(allRooms);
        if (startRoom == null)
            return FindLargestRoom(candidates);

        DungeonRoom bestRoom = null;
        float bestDistance = float.MinValue;
        int bestArea = int.MinValue;

        Vector2 startCenter = startRoom.Bounds.center;

        for (int i = 0; i < candidates.Count; i++)
        {
            DungeonRoom room = candidates[i];
            float distance = Vector2.Distance(startCenter, room.Bounds.center);
            int area = room.Bounds.size.x * room.Bounds.size.y;

            if (distance > bestDistance)
            {
                bestDistance = distance;
                bestArea = area;
                bestRoom = room;
            }
            else if (Mathf.Approximately(distance, bestDistance) && area > bestArea)
            {
                bestArea = area;
                bestRoom = room;
            }
        }

        return bestRoom;
    }

    private DungeonRoom FindStartRoom(List<DungeonRoom> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].RoomType == RoomType.Start)
                return rooms[i];
        }

        return null;
    }
}