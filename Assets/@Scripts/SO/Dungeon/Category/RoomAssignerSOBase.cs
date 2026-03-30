using System.Collections.Generic;
using UnityEngine;

public abstract class RoomAssignerSOBase : ScriptableObject
{
    public enum RoomSelectPolicy
    {
        Largest,
        Smallest,
        Random,
        RandomSmall,
        FarthestFromStart,
        FarthestThenLargest,
        ClosestToCenter
    }

    public abstract void AssignRoomTypes(List<DungeonRoom> rooms);
}