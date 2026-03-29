using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField]
    protected TilemapVisualizer _tilemapVisualizer = null;
    [SerializeField]
    protected Vector2Int _startPosition = Vector2Int.zero;

    public void GenerateDungeon()
    {
        Debug.Log($"[DungeonGenerator] GenerateDungeon 호출 / name = {name}", this);

        _tilemapVisualizer.Clear();
        RunProceduralGeneration();
    }

    protected abstract void RunProceduralGeneration();
}
