using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_BasicRoomAssignRule", menuName = "Scriptable Objects/Dungeon/Room Assigner/Basic Room Assign Rule")]
public class BasicRoomAssignerSO : RoomAssignerSOBase
{
    [SerializeField] private RoomType _targetRoomType = RoomType.Normal;
    [SerializeField] private RoomSelectPolicy _selectPolicy = RoomSelectPolicy.Largest;
    [SerializeField] private Vector2Int _minRoomSize = new Vector2Int(1, 1);
    [SerializeField] private Vector2Int _maxRoomSize = new Vector2Int(999, 999);
    [SerializeField] private bool _excludeStartRoom = true;
    [SerializeField] private bool _onlyNormalRoom = true;
    [SerializeField] private bool _requireDeadEnd = true;

    public Vector2Int MinRoomSize => _minRoomSize;
    public Vector2Int MaxRoomSize => _maxRoomSize;

    public override void AssignRoomTypes(List<DungeonRoom> rooms)
    {
        if (rooms == null || rooms.Count == 0)
            return;

        DungeonRoom targetRoom = FindTargetRoom(rooms);

        if (targetRoom == null && _requireDeadEnd)
        {
            Debug.LogWarning($"[{name}] 막다른 길 조건을 만족하는 방이 없어 조건을 해제합니다.");
            bool originalDeadEnd = _requireDeadEnd;
            _requireDeadEnd = false;

            targetRoom = FindTargetRoom(rooms);

            _requireDeadEnd = originalDeadEnd;
        }

        if (targetRoom == null)
        {
            Debug.LogWarning($"[{name}] 크기 조건을 만족하는 방이 없어 강제 할당합니다.");
            Vector2Int originalMin = _minRoomSize;
            Vector2Int originalMax = _maxRoomSize;
            bool originalDeadEnd = _requireDeadEnd;

            _minRoomSize = new Vector2Int(1, 1);
            _maxRoomSize = new Vector2Int(999, 999);
            _requireDeadEnd = false;

            targetRoom = FindTargetRoom(rooms);

            _minRoomSize = originalMin;
            _maxRoomSize = originalMax;
            _requireDeadEnd = originalDeadEnd;
        }

        if (targetRoom != null)
        {
            targetRoom.RoomType = _targetRoomType;
        }
    }

    private DungeonRoom FindTargetRoom(List<DungeonRoom> rooms)
    {
        List<DungeonRoom> candidates = new List<DungeonRoom>();

        for (int i = 0; i < rooms.Count; i++)
        {
            DungeonRoom room = rooms[i];

            if (_requireDeadEnd && room.ConnectionCount != 1)
                continue;

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

            case RoomSelectPolicy.ClosestToCenter:
                return FindClosestToCenterRoom(candidates, rooms);
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

    private DungeonRoom FindClosestToCenterRoom(List<DungeonRoom> candidates, List<DungeonRoom> allRooms)
    {
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < allRooms.Count; i++)
        {
            BoundsInt b = allRooms[i].Bounds;
            if (b.xMin < min.x) min.x = b.xMin;
            if (b.yMin < min.y) min.y = b.yMin;
            if (b.xMax > max.x) max.x = b.xMax;
            if (b.yMax > max.y) max.y = b.yMax;
        }

        Vector2 dungeonCenter = (min + max) / 2f;

        DungeonRoom bestRoom = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            float distance = Vector2.Distance(candidates[i].Center, dungeonCenter);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestRoom = candidates[i];
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