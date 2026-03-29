using UnityEngine;

[CreateAssetMenu(fileName = "SO_PlayerMovementData", menuName = "Scriptable Objects/Player/Movement")]
public class PlayerMovementSO : ScriptableObject
{
    [field: Header("Move")]
    [field: SerializeField] public float MoveSpeed { get; private set; } = 5f;

    [field: Header("Dash")]
    [field: SerializeField] public float DashSpeed { get; private set; } = 18f;
    [field: SerializeField] public float DashDuration { get; private set; } = 0.14f;
    [field: SerializeField] public float DashCooldown { get; private set; } = 0.45f;
    [field: SerializeField] public bool DashInvincible { get; private set; } = true;
}