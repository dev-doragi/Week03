using System.Collections.Generic;
using UnityEngine;

public static class DungeonStartRoomSelector
{
    public static DungeonRoom FindCenterRoom(
        List<DungeonRoom> rooms,
        BoundsInt dungeonBounds,
        Vector2Int minRoomSize)
    {
        if (rooms == null || rooms.Count == 0)
            return null;

        DungeonRoom bestRoom = null;
        float bestDistance = float.MaxValue;
        Vector2 dungeonCenter = dungeonBounds.center;

        for (int i = 0; i < rooms.Count; i++)
        {
            DungeonRoom room = rooms[i];

            int width = room.Bounds.size.x;
            int height = room.Bounds.size.y;

            if (width < minRoomSize.x || height < minRoomSize.y)
                continue;

            float distance = Vector2.Distance(room.Bounds.center, dungeonCenter);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestRoom = room;
            }
        }

        if (bestRoom != null)
            return bestRoom;

        int bestArea = -1;

        for (int i = 0; i < rooms.Count; i++)
        {
            DungeonRoom room = rooms[i];
            int area = room.Bounds.size.x * room.Bounds.size.y;

            if (area > bestArea)
            {
                bestArea = area;
                bestRoom = room;
            }
        }

        return bestRoom;
    }
}