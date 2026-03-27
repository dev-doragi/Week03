using System.Collections.Generic;
using UnityEngine;

public class RandomPropPlacer : DungeonPropPlacer
{
    protected override void PlaceRoomProps(DungeonLayout layout, DungeonRoom room)
    {
        for (int i = 0; i < _placementSettings.Count; i++)
        {
            PropPlacementSO setting = _placementSettings[i];
            if (setting == null)
                continue;

            if (setting.PlacementMode != PlacementMode.Random)
                continue;

            if (setting.HasValidPrefab() == false)
                continue;

            if (setting.IsAllowedRoom(room.RoomType) == false)
                continue;

            if (Random.value > setting.SpawnChance)
                continue;

            List<Vector2Int> candidates = new List<Vector2Int>(room.InnerTiles);
            candidates.RemoveAll(tile => layout.CorridorTiles.Contains(tile));
            Shuffle(candidates);

            int placedInRoom = 0;
            List<Vector2Int> placedPositions = new();

            for (int j = 0; j < candidates.Count; j++)
            {
                if (HasReachedDungeonLimit(setting))
                    return;

                if (placedInRoom >= setting.MaxPerRoom)
                    break;

                Vector2Int candidate = candidates[j];

                if (HasEnoughSpacing(candidate, placedPositions, setting.MinSpacing) == false)
                    continue;

                if (TryGetFootprintTiles(
                        room,
                        layout.CorridorTiles,
                        candidate,
                        setting.Footprint,
                        PlacementOriginCorner.BottomLeft,
                        out List<Vector2Int> footprintTiles) == false)
                {
                    continue;
                }

                GameObject prefab = setting.GetRandomPrefab();
                if (prefab == null)
                    continue;

                SpawnProp(prefab, candidate);
                OccupyTiles(room, footprintTiles);
                placedPositions.Add(candidate);

                placedInRoom++;
                _placedCountBySetting[setting]++;
            }
        }
    }
}