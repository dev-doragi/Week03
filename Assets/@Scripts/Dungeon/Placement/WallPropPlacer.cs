using System.Collections.Generic;
using UnityEngine;

public class WallPropPlacer : DungeonPropPlacer
{
    protected override void PlaceRoomProps(DungeonLayout layout, DungeonRoom room)
    {
        for (int i = 0; i < _placementSettings.Count; i++)
        {
            PropPlacementSO setting = _placementSettings[i];
            if (setting == null)
                continue;

            if (setting.PlacementMode != PlacementMode.WallInterval)
                continue;

            if (setting.IsAllowedRoom(room.RoomType) == false)
                continue;

            if (Random.value > setting.SpawnChance)
                continue;

            List<WallCandidate> candidates = new();

            AddWallCandidates(candidates, room.NearWallTilesUp, Vector2Int.up, PlacementOriginCorner.TopLeft, setting);
            AddWallCandidates(candidates, room.NearWallTilesDown, Vector2Int.down, PlacementOriginCorner.BottomLeft, setting);
            AddWallCandidates(candidates, room.NearWallTilesLeft, Vector2Int.left, PlacementOriginCorner.BottomLeft, setting);
            AddWallCandidates(candidates, room.NearWallTilesRight, Vector2Int.right, PlacementOriginCorner.BottomRight, setting);

            candidates.RemoveAll(candidate => layout.CorridorTiles.Contains(candidate.Position));
            Shuffle(candidates);

            int placedInRoom = 0;
            List<Vector2Int> placedPositions = new();

            for (int j = 0; j < candidates.Count; j++)
            {
                if (HasReachedDungeonLimit(setting))
                    return;

                if (placedInRoom >= setting.MaxPerRoom)
                    break;

                WallCandidate candidate = candidates[j];

                if (HasEnoughSpacing(candidate.Position, placedPositions, setting.MinSpacing) == false)
                    continue;

                if (TryGetFootprintTiles(
                        room,
                        layout.CorridorTiles,
                        candidate.Position,
                        setting.Footprint,
                        candidate.OriginCorner,
                        out List<Vector2Int> footprintTiles) == false)
                {
                    continue;
                }

                GameObject prefab = setting.GetRandomWallPrefab(candidate.Direction);
                if (prefab == null)
                    continue;

                Quaternion rotation = GetWallRotation(prefab, candidate.Direction);
                Vector3 additionalOffset = GetWallAdditionalOffset(candidate.Direction);

                SpawnProp(prefab, candidate.Position, rotation, additionalOffset);
                OccupyTiles(room, footprintTiles);
                placedPositions.Add(candidate.Position);

                placedInRoom++;
                _placedCountBySetting[setting]++;
            }
        }
    }

    private void AddWallCandidates(
        List<WallCandidate> candidates,
        HashSet<Vector2Int> sourceTiles,
        Vector2Int direction,
        PlacementOriginCorner originCorner,
        PropPlacementSO setting)
    {
        if (sourceTiles == null || sourceTiles.Count == 0)
            return;

        if (setting.HasValidWallPrefab(direction) == false)
            return;

        foreach (Vector2Int tile in sourceTiles)
        {
            candidates.Add(new WallCandidate(tile, direction, originCorner));
        }
    }

    private readonly struct WallCandidate
    {
        public readonly Vector2Int Position;
        public readonly Vector2Int Direction;
        public readonly PlacementOriginCorner OriginCorner;

        public WallCandidate(Vector2Int position, Vector2Int direction, PlacementOriginCorner originCorner)
        {
            Position = position;
            Direction = direction;
            OriginCorner = originCorner;
        }
    }
}