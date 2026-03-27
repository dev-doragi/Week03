using System.Collections.Generic;
using UnityEngine;

public class DungeonLayoutGizmo : MonoBehaviour
{
    [SerializeField] private bool _showGizmo = true;
    [SerializeField] private bool _drawOnlySelected = true;

    [SerializeField] private bool _drawInnerTiles = true;
    [SerializeField] private bool _drawNearWallTilesUp = true;
    [SerializeField] private bool _drawNearWallTilesDown = true;
    [SerializeField] private bool _drawNearWallTilesLeft = true;
    [SerializeField] private bool _drawNearWallTilesRight = true;
    [SerializeField] private bool _drawCornerTiles = true;
    [SerializeField] private bool _excludeCorridorTiles = true;

    [SerializeField] private Vector3 _tileOffset = new Vector3(0.5f, 0.5f, 0f);
    [SerializeField] private Vector3 _tileSize = Vector3.one;

    private DungeonGenerator _dungeonGenerator;

    private void Awake()
    {
        // 같은 오브젝트의 생성기를 캐싱합니다.
        _dungeonGenerator = GetComponent<DungeonGenerator>();
    }

    private void OnDrawGizmos()
    {
        // 선택 상태가 아닐 때도 그릴지 여부를 제어합니다.
        if (_drawOnlySelected)
            return;

        DrawLayoutGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        // 선택된 상태에서만 그릴지 여부를 제어합니다.
        if (_drawOnlySelected == false)
            return;

        DrawLayoutGizmos();
    }

    private void DrawLayoutGizmos()
    {
        // 기즈모 표시가 꺼져 있으면 종료합니다.
        if (_showGizmo == false)
            return;

        // 에디터 리컴파일 직후를 대비해 다시 캐싱합니다.
        if (_dungeonGenerator == null)
        {
            _dungeonGenerator = GetComponent<DungeonGenerator>();
        }

        if (_dungeonGenerator == null)
            return;

        DungeonLayout currentLayout = _dungeonGenerator.CurrentLayout;
        if (currentLayout == null || currentLayout.Rooms == null)
            return;

        HashSet<Vector2Int> corridorTiles = currentLayout.CorridorTiles;

        foreach (DungeonRoom room in currentLayout.Rooms)
        {
            Color roomColor = GetColorForRoomType(room.RoomType);

            if (_drawInnerTiles)
            {
                DrawTiles(room.InnerTiles, corridorTiles, roomColor);
            }

            if (_drawNearWallTilesUp)
            {
                DrawTiles(room.NearWallTilesUp, corridorTiles, roomColor);
            }

            if (_drawNearWallTilesDown)
            {
                DrawTiles(room.NearWallTilesDown, corridorTiles, roomColor);
            }

            if (_drawNearWallTilesRight)
            {
                DrawTiles(room.NearWallTilesRight, corridorTiles, roomColor);
            }

            if (_drawNearWallTilesLeft)
            {
                DrawTiles(room.NearWallTilesLeft, corridorTiles, roomColor);
            }

            if (_drawCornerTiles)
            {
                DrawTiles(room.CornerTiles, corridorTiles, roomColor);
            }
        }
    }

    private Color GetColorForRoomType(RoomType type)
    {
        return type switch
        {
            RoomType.Boss => Color.red,
            RoomType.Treasure => Color.blue,
            RoomType.Normal => Color.green,
            RoomType.Start => Color.pink,
            _ => Color.gray,
        };
    }

    private void DrawTiles(
        HashSet<Vector2Int> tiles,
        HashSet<Vector2Int> corridorTiles,
        Color color)
    {
        if (tiles == null || tiles.Count == 0)
            return;

        Gizmos.color = color;

        foreach (Vector2Int tilePosition in tiles)
        {
            // 복도 타일 제외 옵션이 켜져 있으면 건너뜁니다.
            if (_excludeCorridorTiles && corridorTiles != null && corridorTiles.Contains(tilePosition))
                continue;

            Vector3 drawPosition = new Vector3(tilePosition.x, tilePosition.y, 0f) + _tileOffset;
            Gizmos.DrawCube(drawPosition, _tileSize);
        }
    }
}