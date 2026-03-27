using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="SO_",menuName = "PCG/SimpleRAndomWalkData")]
public class SimpleRandomWalkSO : ScriptableObject
{
    [SerializeField]
    private int _iterations = 10, _walkLength = 10;
    [SerializeField]
    private bool _startRandomlyEachIteration = true;

    public int Iterations => _iterations;
    public int WalkLength => _walkLength;
    public bool StartRandomlyEachIteration => _startRandomlyEachIteration;
}
