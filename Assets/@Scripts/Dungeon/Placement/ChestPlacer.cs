using System.Collections.Generic;
using UnityEngine;

public class ChestPlacer : DungeonPropPlacer
{
    protected override void PlaceRoomProps(DungeonLayout layout, DungeonRoom room)
    {
        for (int i = 0; i < _placementSettings.Count; i++)
        {
            PropPlacementSO setting = _placementSettings[i];
            if (setting == null)
                continue;

            if (setting.PlacementMode != PlacementMode.RoomCenter)
                continue;

            if (setting.HasValidPrefab() == false)
                continue;

            if (setting.IsAllowedRoom(room.RoomType) == false)
                continue;

            if (HasReachedDungeonLimit(setting))
                continue;

            if (Random.value > setting.SpawnChance)
                continue;

            Vector2Int centerTile = room.Center;

            if (TryGetFootprintTiles(
                    room,
                    null,
                    centerTile,
                    setting.Footprint,
                    PlacementOriginCorner.BottomLeft,
                    out List<Vector2Int> footprintTiles) == false)
            {
                continue;
            }

            GameObject prefab = setting.GetRandomPrefab();
            if (prefab == null)
                continue;

            SpawnProp(prefab, centerTile);
            OccupyTiles(room, footprintTiles);
            _placedCountBySetting[setting]++;
        }
    }
}