using System.Collections;
using UnityEngine;

public class RangedEnemy : EnemyBase
{
    [SerializeField] private Transform _firePoint;

    private PoolManager _poolManager;

    protected SO_RangedEnemyData RangedData => EnemyData as SO_RangedEnemyData;

    protected override void Start()
    {
        base.Start();

        ManagerRegistry.TryGet(out _poolManager);

        if (_poolManager != null && RangedData != null && RangedData.ProjectilePrefab != null)
        {
            _poolManager.CreatePool(RangedData.ProjectilePrefab.gameObject);
        }
    }

    protected override void TickCombat(float distance)
    {
        if (RangedData == null)
        {
            StopMovement();
            SetRunAnimation(false);
            return;
        }

        UpdateFacingToTarget();

        if (_isAttacking)
        {
            StopMovement();
            SetRunAnimation(false);
            return;
        }

        if (distance < RangedData.RetreatRange)
        {
            MoveAwayFromTarget();
            SetRunAnimation(true);
        }
        else if (distance > RangedData.PreferredRange)
        {
            MoveTowardsTarget();
            SetRunAnimation(true);
        }
        else
        {
            StopMovement();
            SetRunAnimation(false);
        }

        if (distance <= RangedData.PreferredRange && CanStartAttack())
        {
            StartAttack();
        }
    }

    protected override IEnumerator AttackRoutine()
    {
        if (RangedData == null || RangedData.ProjectilePrefab == null || Target == null)
            yield break;

        StopMovement();
        SetRunAnimation(false);

        if (RangedData.AttackWindup > 0f)
            yield return new WaitForSeconds(RangedData.AttackWindup);

        if (RangedData == null || RangedData.ProjectilePrefab == null || Target == null || IsDead)
            yield break;

        int burstCount = Mathf.Max(1, RangedData.BurstCount);

        for (int i = 0; i < burstCount; i++)
        {
            FireProjectile();

            if (i < burstCount - 1 && RangedData.BurstInterval > 0f)
                yield return new WaitForSeconds(RangedData.BurstInterval);

            if (Target == null || IsDead)
                yield break;
        }
    }

    private void UpdateFacingToTarget()
    {
        if (Target == null)
            return;

        Vector2 direction = ((Vector2)Target.position - _rb.position).normalized;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        SetFacingDirection(direction);
    }

    private void FireProjectile()
    {
        if (RangedData == null || RangedData.ProjectilePrefab == null || Target == null)
            return;

        Vector2 origin = _firePoint != null ? (Vector2)_firePoint.position : _rb.position;
        Vector2 direction = ((Vector2)Target.position - origin).normalized;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        SetFacingDirection(direction);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        GameObject projectileObject = null;

        if (_poolManager != null)
        {
            projectileObject = _poolManager.Get(RangedData.ProjectilePrefab.gameObject, origin, rotation);
        }
        else
        {
            projectileObject = Instantiate(RangedData.ProjectilePrefab.gameObject, origin, rotation);
        }

        if (projectileObject == null)
            return;

        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();

        if (projectile == null)
        {
            if (_poolManager != null)
                _poolManager.Return(projectileObject);
            else
                Destroy(projectileObject);

            return;
        }

        projectile.Initialize(
            direction,
            RangedData.ProjectileSpeed,
            AttackDamage,
            RangedData.ProjectileLifetime,
            gameObject
        );
    }
}