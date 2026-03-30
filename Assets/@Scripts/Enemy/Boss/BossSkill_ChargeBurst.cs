using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSkill_ChargeBurst : BossSkillBase
{
    [Header("Charge Settings")]
    [SerializeField] private float _chargeSpeed = 18f;
    [SerializeField] private float _chargeDuration = 0.35f;
    [SerializeField] private float _wallStopOffset = 0.02f;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private LayerMask _targetMask; // 플레이어 레이어 추가

    private Collider2D _ownerCollider;
    private Coroutine _chargeRoutine;
    private readonly RaycastHit2D[] _castResults = new RaycastHit2D[8]; // 결과 배열 크기 확장
    private readonly HashSet<IDamageable> _hitTargets = new(); // 중복 피격 방지

    public override void Initialize(EnemyBossBase owner)
    {
        base.Initialize(owner);
        _ownerCollider = owner.GetComponent<Collider2D>();
    }

    public override void Enter()
    {
        if (Owner != null && Owner.Rb != null)
        {
            Owner.Rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    public override void Execute()
    {
        if (Owner == null || Owner.Target == null || Owner.Rb == null || _ownerCollider == null)
            return;

        if (_chargeRoutine != null)
            StopCoroutine(_chargeRoutine);

        _hitTargets.Clear(); // 새로운 돌진 시 피격 목록 초기화
        Vector2 chargeDir = ((Vector2)Owner.Target.position - Owner.Rb.position).normalized;
        _chargeRoutine = StartCoroutine(CoCharge(chargeDir));
    }

    public override void Exit()
    {
        if (_chargeRoutine != null)
        {
            StopCoroutine(_chargeRoutine);
            _chargeRoutine = null;
        }

        if (Owner != null && Owner.Rb != null)
        {
            Owner.Rb.linearVelocity = Vector2.zero;
            Owner.Rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        }
    }

    private IEnumerator CoCharge(Vector2 direction)
    {
        float elapsed = 0f;
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        while (elapsed < _chargeDuration)
        {
            if (Owner == null || Owner.Rb == null || _ownerCollider == null)
                yield break;

            float stepDistance = _chargeSpeed * Time.fixedDeltaTime;

            // 1. 플레이어 피격 판정 (TargetMask 체크)
            CheckDamage(direction, stepDistance);

            // 2. 장애물 충돌 판정 (ObstacleMask 체크)
            ContactFilter2D obstacleFilter = new ContactFilter2D();
            obstacleFilter.useLayerMask = true;
            obstacleFilter.layerMask = _obstacleMask;
            obstacleFilter.useTriggers = false;

            int wallHitCount = _ownerCollider.Cast(direction, obstacleFilter, _castResults, stepDistance + _wallStopOffset);

            if (wallHitCount > 0)
            {
                float moveDistance = Mathf.Max(0f, _castResults[0].distance - _wallStopOffset);
                Owner.Rb.MovePosition(Owner.Rb.position + direction * moveDistance);
                Owner.Rb.linearVelocity = Vector2.zero;
                _chargeRoutine = null;
                yield break;
            }

            // 3. 이동 실행
            Vector2 targetPosition = Owner.Rb.position + direction * stepDistance;
            Owner.Rb.MovePosition(targetPosition);

            elapsed += Time.fixedDeltaTime;
            yield return wait;
        }

        if (Owner != null && Owner.Rb != null)
            Owner.Rb.linearVelocity = Vector2.zero;

        _chargeRoutine = null;
    }

    private void CheckDamage(Vector2 direction, float distance)
    {
        ContactFilter2D targetFilter = new ContactFilter2D();
        targetFilter.useLayerMask = true;
        targetFilter.layerMask = _targetMask;
        targetFilter.useTriggers = true; // 플레이어가 Trigger Collider일 수 있으므로 true

        // 돌진 경로상의 타겟 검사
        int hitCount = _ownerCollider.Cast(direction, targetFilter, _castResults, distance);

        for (int i = 0; i < hitCount; i++)
        {
            if (_castResults[i].collider.TryGetComponent(out IDamageable damageable))
            {
                if (_hitTargets.Contains(damageable)) continue;

                // 데미지 입히기 (Owner의 AttackDamage 활용)
                damageable.TakeDamage(Owner.AttackDamage);
                _hitTargets.Add(damageable);

                Debug.Log($"[BossCharge] {_castResults[i].collider.name} 피격! 데미지: {Owner.AttackDamage}");
            }
        }
    }
}