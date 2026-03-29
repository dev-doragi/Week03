using UnityEngine;

[CreateAssetMenu(fileName = "SO_RangedEnemyData", menuName = "Scriptable Objects/Enemy/Ranged Enemy")]
public class SO_RangedEnemyData : SO_EnemyBaseData
{
    [Header("Ranged")]
    [SerializeField] private EnemyProjectile _projectilePrefab;
    [SerializeField] private float _projectileSpeed = 10f;
    [SerializeField] private float _projectileLifetime = 2f;
    [SerializeField] private float _preferredRange = 6f;
    [SerializeField] private float _retreatRange = 2.5f;
    [SerializeField] private float _attackWindup = 0.1f;
    [SerializeField] private int _burstCount = 1;
    [SerializeField] private float _burstInterval = 0.1f;

    public EnemyProjectile ProjectilePrefab => _projectilePrefab;
    public float ProjectileSpeed => _projectileSpeed;
    public float ProjectileLifetime => _projectileLifetime;
    public float PreferredRange => _preferredRange;
    public float RetreatRange => _retreatRange;
    public float AttackWindup => _attackWindup;
    public int BurstCount => _burstCount;
    public float BurstInterval => _burstInterval;
}
