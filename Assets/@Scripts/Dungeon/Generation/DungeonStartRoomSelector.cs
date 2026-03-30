using System.Collections.Generic;
using UnityEngine;

public static class DungeonStartRoomSelector
{
    public static DungeonRoom FindCenterRoom(
        List<DungeonRoom> rooms,
        BoundsInt dungeonBounds,
        Vector2Int minRoomSize,
        Vector2Int maxRoomSize) // 최대 크기 파라미터 추가
    {
        if (rooms == null || rooms.Count == 0)
            return null;

        DungeonRoom bestRoom = null;
        float bestDistance = float.MaxValue;
        Vector2 dungeonCenter = dungeonBounds.center;

        // 1. Min과 Max 조건을 모두 만족하면서 던전 중앙에 가장 가까운 방 찾기
        for (int i = 0; i < rooms.Count; i++)
        {
            DungeonRoom room = rooms[i];

            int width = room.Bounds.size.x;
            int height = room.Bounds.size.y;

            // 최소 크기 제약
            if (width < minRoomSize.x || height < minRoomSize.y)
                continue;

            // 최대 크기 제약 (추가됨: 너무 큰 방은 시작 방 후보에서 제외)
            if (width > maxRoomSize.x || height > maxRoomSize.y)
                continue;

            float distance = Vector2.Distance(room.Bounds.center, dungeonCenter);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestRoom = room;
            }
        }

        // 조건을 완벽히 만족하는 방을 찾았다면 반환
        if (bestRoom != null)
            return bestRoom;

        // 2. [안전장치] 해당 크기 조건을 만족하는 방이 맵에 하나도 없다면, 
        // 크기 제약을 무시하고 무조건 중앙에서 가장 가까운 방을 할당합니다.
        bestDistance = float.MaxValue;
        for (int i = 0; i < rooms.Count; i++)
        {
            DungeonRoom room = rooms[i];
            float distance = Vector2.Distance(room.Bounds.center, dungeonCenter);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestRoom = room;
            }
        }

        return bestRoom;
    }
}