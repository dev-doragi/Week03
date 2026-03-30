using UnityEngine;

public class BossSkill_ProjectileBurst : BossSkillBase
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private int _projectileCount = 8;
    [SerializeField] private float _spreadAngle = 60f;
    [SerializeField] private float _projectileSpeed = 7f;

    private PoolManager _pool;

    public override void Initialize(EnemyBossBase owner)
    {
        base.Initialize(owner);
        // 프로젝트 공통 방식인 ManagerRegistry를 통해 풀 가져오기
        ManagerRegistry.TryGet(out _pool);

        if (_pool != null && _projectilePrefab != null)
        {
            _pool.CreatePool(_projectilePrefab); // 미리 풀 생성
        }
    }

    public override void Enter() { }

    public override void Execute()
    {
        if (Owner.Target == null || _projectilePrefab == null) return;

        Vector2 dirToTarget = (Owner.Target.position - transform.position).normalized;
        float centerAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        float startAngle = centerAngle - (_spreadAngle / 2f);

        for (int i = 0; i < _projectileCount; i++)
        {
            float currentAngle = _spreadAngle >= 360f
                ? startAngle + (360f / _projectileCount) * i
                : startAngle + (_spreadAngle / Mathf.Max(1, _projectileCount - 1)) * i;

            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            Vector2 moveDir = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));

            GameObject go;
            // 풀 매니저를 사용하여 오브젝트 획득
            if (_pool != null)
            {
                go = _pool.Get(_projectilePrefab, _firePoint.position, rotation);
            }
            else
            {
                go = Instantiate(_projectilePrefab, _firePoint.position, rotation);
            }

            if (go.TryGetComponent(out EnemyProjectile proj))
            {
                proj.Initialize(moveDir, _projectileSpeed, Owner.AttackDamage, 5f, Owner.gameObject); //
            }
        }
    }

    public override void Exit() { }
}