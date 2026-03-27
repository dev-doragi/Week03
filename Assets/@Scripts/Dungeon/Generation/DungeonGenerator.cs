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

    [SerializeField] private Vector2Int _startRoomMinSize = new Vector2Int(8, 8);
    public DungeonRoom StartRoom { get; private set; }
    public Vector2Int PlayerSpawnTile { get; private set; }

    [SerializeField] private List<DungeonPropPlacer> _propPlacers = new();

    [SerializeField] private RoomAssignerSOBase[] _roomAssigners;

    public DungeonLayout CurrentLayout { get; private set; }

    public event Action<DungeonLayout> OnDungeonGenerated;

    protected override void RunProceduralGeneration()
    {
        // RoomFirst 기반 레이아웃을 생성합니다.
        CurrentLayout = CreateDungeonLayout();
        if (CurrentLayout == null)
            return;

        // 생성된 바닥 타일을 렌더링합니다.
        _tilemapVisualizer.PaintFloorTiles(CurrentLayout.FloorTiles);

        // 바닥 기준으로 벽을 생성합니다.
        WallGenerator.CreateWalls(CurrentLayout.FloorTiles, _tilemapVisualizer);

        // 배치기가 연결되어 있으면 프리팹 배치를 수행합니다.
        for (int i = 0; i < _propPlacers.Count; i++)
        {
            if (_propPlacers[i] != null)
            {
                _propPlacers[i].PlaceProps(CurrentLayout);
            }
        }

        OnDungeonGenerated?.Invoke(CurrentLayout);
    }

    private DungeonLayout CreateDungeonLayout()
    {
        BoundsInt dungeonBounds = new BoundsInt(
            (Vector3Int)_startPosition,
            new Vector3Int(_dungeonWidth, _dungeonHeight, 0));

        // BSP로 방 후보 영역을 분할합니다.
        List<BoundsInt> roomBoundsList =
            ProceduralGenerationAlgorithms.BinarySpacePartitioning(
                dungeonBounds,
                _minRoomWidth,
                _minRoomHeight);

        List<DungeonRoom> rooms = _randomWalkRooms
            ? CreateRandomWalkRooms(roomBoundsList)
            : CreateSimpleRooms(roomBoundsList);

        StartRoom = DungeonStartRoomSelector.FindCenterRoom(rooms, dungeonBounds, _startRoomMinSize);

        if (StartRoom != null)
        {
            StartRoom.RoomType = RoomType.Start;
            PlayerSpawnTile = StartRoom.Center;
        }

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

        List<Vector2Int> roomCenters = new List<Vector2Int>();
        for (int i = 0; i < rooms.Count; i++)
        {
            roomCenters.Add(rooms[i].Center);
        }

        // 방 중심끼리 복도를 연결합니다.
        HashSet<Vector2Int> corridorTiles = ConnectRooms(roomCenters, StartRoom != null ? StartRoom.Center : roomCenters[0]);

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

        // 방 내부 분석 데이터를 생성합니다.
        DungeonLayoutAnalyzer.Analyze(layout);

        return layout;
    }

    private List<DungeonRoom> CreateSimpleRooms(List<BoundsInt> roomBoundsList)
    {
        List<DungeonRoom> rooms = new List<DungeonRoom>();

        for (int i = 0; i < roomBoundsList.Count; i++)
        {
            BoundsInt roomBounds = roomBoundsList[i];
            HashSet<Vector2Int> roomFloor = CreateSimpleRoomFloor(roomBounds);
            Vector2Int roomCenter = new Vector2Int(
                Mathf.RoundToInt(roomBounds.center.x),
                Mathf.RoundToInt(roomBounds.center.y));

            rooms.Add(new DungeonRoom(roomBounds, roomCenter, roomFloor));
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

            // 기존 Random Walk 결과를 방 영역 내부로 잘라냅니다.
            HashSet<Vector2Int> randomWalkFloor = RunRandomWalk(_randomWalkParameters, roomCenter);
            HashSet<Vector2Int> clippedFloor = ClipFloorToBounds(randomWalkFloor, roomBounds);

            // Random Walk 결과가 비면 단순 직사각형 방으로 대체합니다.
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

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters, Vector2Int startCenter)
    {
        HashSet<Vector2Int> corridorTiles = new HashSet<Vector2Int>();
        if (roomCenters.Count == 0)
            return corridorTiles;

        List<Vector2Int> remainingCenters = new List<Vector2Int>(roomCenters);
        Vector2Int currentRoomCenter = startCenter;
        remainingCenters.Remove(currentRoomCenter);

        while (remainingCenters.Count > 0)
        {
            Vector2Int closestCenter = FindClosestPointTo(currentRoomCenter, remainingCenters);
            remainingCenters.Remove(closestCenter);

            // 가장 가까운 방과 현재 방을 연결합니다.
            HashSet<Vector2Int> corridor = CreateCorridor(currentRoomCenter, closestCenter);
            corridorTiles.UnionWith(corridor);

            currentRoomCenter = closestCenter;
        }

        return corridorTiles;
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

    private Vector2Int FindClosestPointTo(
        Vector2Int currentRoomCenter,
        List<Vector2Int> roomCenters)
    {
        Vector2Int closestPoint = Vector2Int.zero;
        float shortestDistance = float.MaxValue;

        for (int i = 0; i < roomCenters.Count; i++)
        {
            float currentDistance = Vector2.Distance(roomCenters[i], currentRoomCenter);

            if (currentDistance < shortestDistance)
            {
                shortestDistance = currentDistance;
                closestPoint = roomCenters[i];
            }
        }

        return closestPoint;
    }
}