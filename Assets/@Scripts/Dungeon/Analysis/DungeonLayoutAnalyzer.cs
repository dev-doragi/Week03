using UnityEngine;

public static class DungeonLayoutAnalyzer
{
    public static void Analyze(DungeonLayout layout)
    {
        if (layout == null)
            return;

        for (int i = 0; i < layout.Rooms.Count; i++)
        {
            AnalyzeRoom(layout.Rooms[i]);
        }
    }

    private static void AnalyzeRoom(DungeonRoom room)
    {
        room.ClearAnalysis();

        foreach (Vector2Int tilePosition in room.FloorTiles)
        {
            int neighbourCount = 0;

            bool hasUp = room.FloorTiles.Contains(tilePosition + Vector2Int.up);
            bool hasDown = room.FloorTiles.Contains(tilePosition + Vector2Int.down);
            bool hasLeft = room.FloorTiles.Contains(tilePosition + Vector2Int.left);
            bool hasRight = room.FloorTiles.Contains(tilePosition + Vector2Int.right);

            // 상단 인접 타일 여부를 기록합니다.
            if (hasUp)
            {
                neighbourCount++;
            }
            else
            {
                room.NearWallTilesUp.Add(tilePosition);
            }

            // 하단 인접 타일 여부를 기록합니다.
            if (hasDown)
            {
                neighbourCount++;
            }
            else
            {
                room.NearWallTilesDown.Add(tilePosition);
            }

            // 좌측 인접 타일 여부를 기록합니다.
            if (hasLeft)
            {
                neighbourCount++;
            }
            else
            {
                room.NearWallTilesLeft.Add(tilePosition);
            }

            // 우측 인접 타일 여부를 기록합니다.
            if (hasRight)
            {
                neighbourCount++;
            }
            else
            {
                room.NearWallTilesRight.Add(tilePosition);
            }

            // 코너 또는 내부 타일을 분류합니다.
            if (neighbourCount <= 2)
            {
                room.CornerTiles.Add(tilePosition);
            }
            else if (neighbourCount == 4)
            {
                room.InnerTiles.Add(tilePosition);
            }
        }

        // 코너 타일은 벽 인접 목록에서 제거합니다.
        room.NearWallTilesUp.ExceptWith(room.CornerTiles);
        room.NearWallTilesDown.ExceptWith(room.CornerTiles);
        room.NearWallTilesLeft.ExceptWith(room.CornerTiles);
        room.NearWallTilesRight.ExceptWith(room.CornerTiles);
    }
}