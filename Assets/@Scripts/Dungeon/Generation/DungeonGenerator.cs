using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField] private int _minRoomWidth = 4;
    [SerializeField] private int _minRoomHeight = 4;
    [SerializeField] private int _dungeonWidth = 20;
    [SerializeField] private int _dungeonHeight = 20;
    [SerializeField][Range(0, 10)] private int _offset = 1;
    [SerializeField] private bool _randomWalkRooms = false;

    public DungeonRoom StartRoom { get; private set; }
    public Vector2Int PlayerSpawnTile { get; private set; }

    [SerializeField] private List<DungeonPropPlacer> _propPlacers = new();
    [SerializeField] private RoomAssignerSOBase[] _roomAssigners;

    public DungeonLayout CurrentLayout { get; private set; }
    public event Action<DungeonLayout> OnDungeonGenerated;

    protected override void RunProceduralGeneration()
    {
        CurrentLayout = CreateDungeonLayout();
        if (CurrentLayout == null)
            return;

        _tilemapVisualizer.PaintFloorTiles(CurrentLayout.FloorTiles);
        WallGenerator.CreateWalls(CurrentLayout.FloorTiles, _tilemapVisualizer);

        for (int i = 0; i < _propPlacers.Count; i++)
        {
            if (_propPlacers[i] != null)
            {
                _propPlacers[i].PlaceProps(CurrentLayout);
            }
        }

        Debug.Log($"[DungeonGenerator] OnDungeonGenerated Invoke / name = {name}", this);
        OnDungeonGenerated?.Invoke(CurrentLayout);
    }

    private DungeonLayout CreateDungeonLayout()
    {
        BoundsInt dungeonBounds = new BoundsInt(
            (Vector3Int)_startPosition,
            new Vector3Int(_dungeonWidth, _dungeonHeight, 0));

        List<BoundsInt> roomBoundsList =
            ProceduralGenerationAlgorithms.BinarySpacePartitioning(
                dungeonBounds,
                _minRoomWidth,
                _minRoomHeight);

        List<DungeonRoom> rooms = _randomWalkRooms
            ? CreateRandomWalkRooms(roomBoundsList)
            : CreateSimpleRooms(roomBoundsList);

        if (rooms.Count == 0)
            return null;

        // 시작점 할당 전에 방들을 연결하여 ConnectionCount 계산
        HashSet<Vector2Int> corridorTiles = ConnectRoomsByObject(rooms, rooms[0]);

        // SO 정책에 따라 방 할당
        if (_roomAssigners != null)
        {
            for (int i = 0; i < _roomAssigners.Length; i++)
            {
                if (_roomAssigners[i] != null)
                {
                    _roomAssigners[i].AssignRoomTypes(rooms);
                }
            }
        }

        // 할당된 방 중 StartRoom 탐색
        StartRoom = null;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].RoomType == RoomType.Start)
            {
                StartRoom = rooms[i];
                PlayerSpawnTile = StartRoom.Center;
                break;
            }
        }

        // 안전장치: SO에서 StartRoom이 지정되지 않은 경우 임의 할당
        if (StartRoom == null)
        {
            StartRoom = rooms[0];
            StartRoom.RoomType = RoomType.Start;
            PlayerSpawnTile = StartRoom.Center;
        }

        HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>(corridorTiles);

        for (int i = 0; i < rooms.Count; i++)
        {
            floorTiles.UnionWith(rooms[i].FloorTiles);
        }

        DungeonLayout layout = new DungeonLayout(
            _startPosition,
            floorTiles,
            corridorTiles,
            rooms);

        DungeonLayoutAnalyzer.Analyze(layout);

        return layout;
    }

    private List<DungeonRoom> CreateSimpleRooms(List<BoundsInt> roomBoundsList)
    {
        List<DungeonRoom> rooms = new List<DungeonRoom>();

        List<BasicRoomAssignerSO> requiredSOs = new List<BasicRoomAssignerSO>();
        if (_roomAssigners != null)
        {
            for (int i = 0; i < _roomAssigners.Length; i++)
            {
                if (_roomAssigners[i] is BasicRoomAssignerSO basicSO)
                {
                    if (basicSO.MinRoomSize.x > 1 || basicSO.MaxRoomSize.x < 999)
                    {
                        requiredSOs.Add(basicSO);
                    }
                }
            }
            requiredSOs.Sort((a, b) => (b.MinRoomSize.x * b.MinRoomSize.y).CompareTo(a.MinRoomSize.x * a.MinRoomSize.y));
        }

        for (int i = 0; i < roomBoundsList.Count; i++)
        {
            BoundsInt nodeBounds = roomBoundsList[i];

            int roomWidth = nodeBounds.size.x;
            int roomHeight = nodeBounds.size.y;
            int xOffset = 0;
            int yOffset = 0;

            for (int j = 0; j < requiredSOs.Count; j++)
            {
                Vector2Int minReq = requiredSOs[j].MinRoomSize;
                Vector2Int maxReq = requiredSOs[j].MaxRoomSize;

                if (nodeBounds.size.x >= minReq.x && nodeBounds.size.y >= minReq.y)
                {
                    roomWidth = Mathf.Clamp(nodeBounds.size.x, minReq.x, maxReq.x);
                    roomHeight = Mathf.Clamp(nodeBounds.size.y, minReq.y, maxReq.y);

                    xOffset = (nodeBounds.size.x - roomWidth) / 2;
                    yOffset = (nodeBounds.size.y - roomHeight) / 2;

                    requiredSOs.RemoveAt(j);
                    break;
                }
            }

            BoundsInt actualRoomBounds = new BoundsInt(
                new Vector3Int(nodeBounds.min.x + xOffset, nodeBounds.min.y + yOffset, 0),
                new Vector3Int(roomWidth, roomHeight, 0)
            );

            HashSet<Vector2Int> roomFloor = CreateSimpleRoomFloor(actualRoomBounds);
            Vector2Int roomCenter = new Vector2Int(
                Mathf.RoundToInt(actualRoomBounds.center.x),
                Mathf.RoundToInt(actualRoomBounds.center.y));

            rooms.Add(new DungeonRoom(actualRoomBounds, roomCenter, roomFloor));
        }

        return rooms;
    }

    private List<DungeonRoom> CreateRandomWalkRooms(List<BoundsInt> roomBoundsList)
    {
        List<DungeonRoom> rooms = new List<DungeonRoom>();

        for (int i = 0; i < roomBoundsList.Count; i++)
        {
            BoundsInt roomBounds = roomBoundsList[i];
            Vector2Int roomCenter = new Vector2Int(
                Mathf.RoundToInt(roomBounds.center.x),
                Mathf.RoundToInt(roomBounds.center.y));

            HashSet<Vector2Int> randomWalkFloor = RunRandomWalk(_randomWalkParameters, roomCenter);
            HashSet<Vector2Int> clippedFloor = ClipFloorToBounds(randomWalkFloor, roomBounds);

            if (clippedFloor.Count == 0)
            {
                clippedFloor = CreateSimpleRoomFloor(roomBounds);
            }

            rooms.Add(new DungeonRoom(roomBounds, roomCenter, clippedFloor));
        }

        return rooms;
    }

    private HashSet<Vector2Int> CreateSimpleRoomFloor(BoundsInt roomBounds)
    {
        HashSet<Vector2Int> roomFloor = new HashSet<Vector2Int>();

        for (int x = _offset; x < roomBounds.size.x - _offset; x++)
        {
            for (int y = _offset; y < roomBounds.size.y - _offset; y++)
            {
                Vector2Int tilePosition = (Vector2Int)roomBounds.min + new Vector2Int(x, y);
                roomFloor.Add(tilePosition);
            }
        }

        return roomFloor;
    }

    private HashSet<Vector2Int> ClipFloorToBounds(
        HashSet<Vector2Int> floorTiles,
        BoundsInt roomBounds)
    {
        HashSet<Vector2Int> clippedFloor = new HashSet<Vector2Int>();

        foreach (Vector2Int tilePosition in floorTiles)
        {
            bool insideX =
                tilePosition.x >= roomBounds.xMin + _offset &&
                tilePosition.x <= roomBounds.xMax - _offset;

            bool insideY =
                tilePosition.y >= roomBounds.yMin + _offset &&
                tilePosition.y <= roomBounds.yMax - _offset;

            if (insideX && insideY)
            {
                clippedFloor.Add(tilePosition);
            }
        }

        return clippedFloor;
    }

    private HashSet<Vector2Int> ConnectRoomsByObject(List<DungeonRoom> rooms, DungeonRoom startRoom)
    {
        HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();
        if (rooms.Count == 0) return corridorTiles;

        List<DungeonRoom> remainingRooms = new List<DungeonRoom>(rooms);
        DungeonRoom currentRoom = startRoom ?? rooms[0];
        remainingRooms.Remove(currentRoom);

        while (remainingRooms.Count > 0)
        {
            DungeonRoom closestRoom = FindClosestRoomTo(currentRoom, remainingRooms);
            remainingRooms.Remove(closestRoom);

            currentRoom.ConnectionCount++;
            closestRoom.ConnectionCount++;

            HashSet<Vector2Int> corridor = CreateCorridor(currentRoom.Center, closestRoom.Center);
            corridorTiles.UnionWith(corridor);

            currentRoom = closestRoom;
        }

        return corridorTiles;
    }

    private DungeonRoom FindClosestRoomTo(DungeonRoom currentRoom, List<DungeonRoom> rooms)
    {
        DungeonRoom closestRoom = null;
        float shortestDistance = float.MaxValue;

        for (int i = 0; i < rooms.Count; i++)
        {
            float currentDistance = Vector2.Distance(rooms[i].Center, currentRoom.Center);

            if (currentDistance < shortestDistance)
            {
                shortestDistance = currentDistance;
                closestRoom = rooms[i];
            }
        }

        return closestRoom;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        Vector2Int position = currentRoomCenter;
        corridor.Add(position);

        while (position.y != destination.y)
        {
            position += destination.y > position.y
                ? Vector2Int.up
                : Vector2Int.down;

            corridor.Add(position);
        }

        while (position.x != destination.x)
        {
            position += destination.x > position.x
                ? Vector2Int.right
                : Vector2Int.left;

            corridor.Add(position);
        }

        return corridor;
    }
}