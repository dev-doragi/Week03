using UnityEngine;

[CreateAssetMenu(fileName = "SO_WeaponData", menuName = "Scriptable Objects/Player/Weapon")]
public class WeaponSO : ScriptableObject
{
    [field: Header("Combat Stats")]
    [field: SerializeField] public float AttackInterval { get; private set; } = 0.12f;
    [field: SerializeField] public int Damage { get; private set; } = 1;
    [field: SerializeField] public float ProjectileSpeed { get; private set; } = 18f;
    [field: SerializeField] public float ProjectileLifetime { get; private set; } = 2f;
    [field: SerializeField] public int MaxAmmo { get; private set; } = 6;
    [field: SerializeField] public float ReloadDuration { get; private set; } = 1f;
    [field: SerializeField] public GameObject ProjectilePrefab { get; private set; }
    [field: SerializeField] public float SpreadAngle { get; private set; } = 0f;
    [field: SerializeField] public int PelletCount { get; private set; } = 1;

    [field: Header("Visual & Prefab")]
    // [추가] 무기의 외형과 총구(Muzzle)가 세팅된 프리팹
    [field: SerializeField] public GameObject WeaponPrefab { get; private set; }
}