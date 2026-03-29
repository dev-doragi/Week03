using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    [SerializeField] private Transform _attackPivot;
    [SerializeField] private Transform _attackPoint;

    protected SO_MeleeEnemyData MeleeData => EnemyData as SO_MeleeEnemyData;

    // Debug
    protected override void Awake()
    {
        base.Awake();

        var player = FindAnyObjectByType<PlayerController>();
        SetTarget(player.gameObject.transform);
    }

    protected override void TickCombat(float distance)
    {
        if (MeleeData == null)
        {
            StopMovement();
            SetRunAnimation(false);
            return;
        }

        UpdateAttackPivot();

        if (Target != null)
        {
            Vector2 direction = ((Vector2)Target.position - _rb.position).normalized;
            SetFacingDirection(direction);
        }

        if (_isAttacking)
        {
            StopMovement();
            SetRunAnimation(true);
            return;
        }

        if (distance <= AttackRange)
        {
            StopMovement();
            SetRunAnimation(true);

            if (CanStartAttack())
            {
                StartAttack();
            }

            return;
        }

        MoveTowardsTarget();
        SetRunAnimation(true);
    }

    protected override IEnumerator AttackRoutine()
    {
        if (MeleeData == null)
            yield break;

        yield return new WaitForSeconds(MeleeData.AttackWindup);

        UpdateAttackPivot();
        SpawnSlashEffect();

        Vector2 origin = _attackPoint != null ? (Vector2)_attackPoint.position : _rb.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, MeleeData.AttackRadius, MeleeData.TargetMask);
        HashSet<IDamageable> damagedTargets = new();

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].TryGetComponent(out IDamageable damageable) == false)
                continue;

            if (damagedTargets.Contains(damageable))
                continue;

            damagedTargets.Add(damageable);
            damageable.TakeDamage(AttackDamage);
        }
    }

    private void UpdateAttackPivot()
    {
        if (_attackPivot == null || Target == null)
            return;

        Vector2 direction = ((Vector2)Target.position - _rb.position).normalized;
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        _attackPivot.right = direction;
    }

    private void SpawnSlashEffect()
    {
        if (MeleeData == null || MeleeData.SlashEffectPrefab == null)
            return;

        Vector3 spawnPosition = _attackPoint != null ? _attackPoint.position : transform.position;
        Vector2 direction = _attackPoint != null ? (Vector2)_attackPoint.right : FacingDirection;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector2.right;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + MeleeData.SlashAngleOffset;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        GameObject effectObject = Instantiate(MeleeData.SlashEffectPrefab, spawnPosition, rotation);

        if (MeleeData.SlashEffectLifetime > 0f)
        {
            Destroy(effectObject, MeleeData.SlashEffectLifetime);
        }
    }
}
