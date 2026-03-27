using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField]
    private Tilemap _floorTilemap, _wallTilemap;
    [SerializeField]
    private TileBase _floorTile, _wallTop, _wallSideRight, _wallSiderLeft, _wallBottom, _wallFull, 
        _wallInnerCornerDownLeft, _wallInnerCornerDownRight, 
        _wallDiagonalCornerDownRight, _wallDiagonalCornerDownLeft, _wallDiagonalCornerUpRight, _wallDiagonalCornerUpLeft;

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, _floorTilemap, _floorTile);
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, tile, position);
        }
    }

    internal void PaintSingleBasicWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;
        if (WallTypesHelper.WallTop.Contains(typeAsInt))
        {
            tile = _wallTop;
        }else if (WallTypesHelper.WallSideRight.Contains(typeAsInt))
        {
            tile = _wallSideRight;
        }
        else if (WallTypesHelper.WallSideLeft.Contains(typeAsInt))
        {
            tile = _wallSiderLeft;
        }
        else if (WallTypesHelper.WallBottom.Contains(typeAsInt))
        {
            tile = _wallBottom;
        }
        else if (WallTypesHelper.WallFull.Contains(typeAsInt))
        {
            tile = _wallFull;
        }

        if (tile!=null)
            PaintSingleTile(_wallTilemap, tile, position);
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    public void Clear()
    {
        _floorTilemap.ClearAllTiles();
        _wallTilemap.ClearAllTiles();
    }

    internal void PaintSingleCornerWall(Vector2Int position, string binaryType)
    {
        int typeASInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;

        if (WallTypesHelper.WallInnerCornerDownLeft.Contains(typeASInt))
        {
            tile = _wallInnerCornerDownLeft;
        }
        else if (WallTypesHelper.WallInnerCornerDownRight.Contains(typeASInt))
        {
            tile = _wallInnerCornerDownRight;
        }
        else if (WallTypesHelper.WallDiagonalCornerDownLeft.Contains(typeASInt))
        {
            tile = _wallDiagonalCornerDownLeft;
        }
        else if (WallTypesHelper.WallDiagonalCornerDownRight.Contains(typeASInt))
        {
            tile = _wallDiagonalCornerDownRight;
        }
        else if (WallTypesHelper.WallDiagonalCornerUpRight.Contains(typeASInt))
        {
            tile = _wallDiagonalCornerUpRight;
        }
        else if (WallTypesHelper.WallDiagonalCornerUpLeft.Contains(typeASInt))
        {
            tile = _wallDiagonalCornerUpLeft;
        }
        else if (WallTypesHelper.WallFullEightDirections.Contains(typeASInt))
        {
            tile = _wallFull;
        }
        else if (WallTypesHelper.WallBottmEightDirections.Contains(typeASInt))
        {
            tile = _wallBottom;
        }

        if (tile != null)
            PaintSingleTile(_wallTilemap, tile, position);
    }
}
