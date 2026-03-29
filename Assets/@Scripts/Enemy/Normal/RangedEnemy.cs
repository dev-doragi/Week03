using System.Collections;
using UnityEngine;

public class RangedEnemy : EnemyBase
{
    [SerializeField] private Transform _firePoint;

    protected SO_RangedEnemyData RangedData => EnemyData as SO_RangedEnemyData;

    protected override void TickCombat(float distance)
    {
        if (RangedData == null)
        {
            StopMovement();
            SetRunAnimation(false);
            return;
        }

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

        if (distance <= AttackRange && CanStartAttack())
        {
            StartAttack();
        }
    }

    protected override IEnumerator AttackRoutine()
    {
        if (RangedData == null || RangedData.ProjectilePrefab == null || Target == null)
            yield break;

        StopMovement();
        yield return new WaitForSeconds(RangedData.AttackWindup);

        Vector2 origin = _firePoint != null ? (Vector2)_firePoint.position : _rb.position;
        Vector2 direction = ((Vector2)Target.position - origin).normalized;
        Quaternion rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        for (int i = 0; i < RangedData.BurstCount; i++)
        {
            EnemyProjectile projectile = Instantiate(RangedData.ProjectilePrefab, origin, rotation);
            projectile.Initialize(direction, RangedData.ProjectileSpeed, AttackDamage, RangedData.ProjectileLifetime, gameObject);

            if (i < RangedData.BurstCount - 1)
            {
                yield return new WaitForSeconds(RangedData.BurstInterval);
            }
        }
    }
}
