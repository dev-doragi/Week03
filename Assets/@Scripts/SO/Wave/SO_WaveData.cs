using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_WaveData", menuName = "Scriptable Objects/Wave/Wave Data")]
public class SO_WaveData : ScriptableObject
{
    public int id;
    public List<GameObject> enemyPrefabs = new();
    public bool spawnSequentially = false;
    public float spawnInterval = 0.2f;
    public bool isBossWave = false;
}