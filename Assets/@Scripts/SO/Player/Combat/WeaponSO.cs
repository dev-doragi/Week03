using UnityEngine;

[CreateAssetMenu(fileName = "SO_WeaponData", menuName = "SO/Player/Weapon")]
public class WeaponSO : ScriptableObject
{
    [field: SerializeField] public float AttackInterval { get; private set; } = 0.12f;
    [field: SerializeField] public int Damage { get; private set; } = 1;
    [field: SerializeField] public float ProjectileSpeed { get; private set; } = 18f;
    [field: SerializeField] public float ProjectileLifetime { get; private set; } = 2f;
    [field: SerializeField] public int MaxAmmo { get; private set; } = 6;
    [field: SerializeField] public float ReloadDuration { get; private set; } = 1f;
    [field: SerializeField] public GameObject ProjectilePrefab { get; private set; }
    [field: SerializeField] public float SpreadAngle { get; private set; } = 0f;
    [field: SerializeField] public int PelletCount { get; private set; } = 1;
}