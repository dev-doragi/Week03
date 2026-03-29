using UnityEngine;

[CreateAssetMenu(fileName = "SO_MeleeEnemyData", menuName = "Scriptable Objects/Enemy/Melee Enemy")]
public class SO_MeleeEnemyData : SO_EnemyBaseData
{
    [Header("Melee")]
    [SerializeField] private float _attackRadius = 0.75f;
    [SerializeField] private float _attackWindup = 0.12f;
    [SerializeField] private LayerMask _targetMask;

    [Header("Slash Effect")]
    [SerializeField] private GameObject _slashEffectPrefab;
    [SerializeField] private float _slashEffectLifetime = 0.2f;
    [SerializeField] private float _slashAngleOffset = 0f;

    public float AttackRadius => _attackRadius;
    public float AttackWindup => _attackWindup;
    public LayerMask TargetMask => _targetMask;
    public GameObject SlashEffectPrefab => _slashEffectPrefab;
    public float SlashEffectLifetime => _slashEffectLifetime;
    public float SlashAngleOffset => _slashAngleOffset;
}
