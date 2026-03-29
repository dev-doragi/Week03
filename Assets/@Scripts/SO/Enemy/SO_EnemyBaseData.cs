using UnityEngine;

public abstract class SO_EnemyBaseData : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private int _maxHp = 5;
    [SerializeField] private int _attackDamage = 1;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _detectionRange = 8f;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _attackCooldown = 1.2f;
    [SerializeField] private float _deathDespawnDelay = 0.35f;

    [Header("Collision")]
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private LayerMask _enemyMask;
    [SerializeField] private float _separationRadius = 0.7f;
    [SerializeField] private float _separationWeight = 0.8f;

    [Header("Visual")]
    [SerializeField] private Sprite _hitSprite;
    [SerializeField] private Sprite _deathSprite;
    [SerializeField] private float _hitSpriteDuration = 0.08f;
    [SerializeField] private float _flipThreshold = 0.01f;

    public int MaxHp => _maxHp;
    public int AttackDamage => _attackDamage;
    public float MoveSpeed => _moveSpeed;
    public float DetectionRange => _detectionRange;
    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public float DeathDespawnDelay => _deathDespawnDelay;
    public LayerMask ObstacleMask => _obstacleMask;
    public LayerMask EnemyMask => _enemyMask;
    public float SeparationRadius => _separationRadius;
    public float SeparationWeight => _separationWeight;
    public Sprite HitSprite => _hitSprite;
    public Sprite DeathSprite => _deathSprite;
    public float HitSpriteDuration => _hitSpriteDuration;
    public float FlipThreshold => _flipThreshold;
}
