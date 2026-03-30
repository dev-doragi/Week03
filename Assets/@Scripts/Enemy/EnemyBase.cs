using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : EntityBase
{
    [SerializeField] private SO_EnemyBaseData _enemyData;
    [SerializeField] private Collider2D _bodyCollider;

    protected Transform _target;
    protected bool _isAttacking;

    private float _nextAttackTime;
    private bool _hasMoveDirection;
    private Vector2 _moveDirection;
    private Coroutine _attackRoutine;
    private Coroutine _deathRoutine;
    private bool _shouldPlayRunAnimation;
    private Vector2 _facingDirection = Vector2.right;
    private GameStateManager _gameStateManager;

    public event Action<EnemyBase> OnDeathFinished;

    public Transform Target => _target;
    public bool ShouldPlayRunAnimation => _shouldPlayRunAnimation;
    public Vector2 FacingDirection => _facingDirection;
    public SO_EnemyBaseData EnemyData => _enemyData;
    public int AttackDamage => _enemyData != null ? _enemyData.AttackDamage : 0;
    public float AttackRange => _enemyData != null ? _enemyData.AttackRange : 0f;
    public float MoveSpeed => _enemyData != null ? _enemyData.MoveSpeed : 0f;

    protected virtual void Start()
    {
        ManagerRegistry.TryGet(out _gameStateManager);

        _bodyCollider = GetComponent<BoxCollider2D>();
    }

    protected override int GetMaxHp()
    {
        return _enemyData != null ? _enemyData.MaxHp : 1;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    protected override void ResetEntityState()
    {
        base.ResetEntityState();

        _target = null;
        _isAttacking = false;
        _nextAttackTime = 0f;
        _hasMoveDirection = false;
        _moveDirection = Vector2.zero;
        _shouldPlayRunAnimation = false;
        _facingDirection = Vector2.right;

        if (_bodyCollider != null)
            _bodyCollider.enabled = true;

        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        if (_deathRoutine != null)
        {
            StopCoroutine(_deathRoutine);
            _deathRoutine = null;
        }
    }

    protected virtual void Update()
    {
        if (IsDead)
            return;

        if (_gameStateManager != null && _gameStateManager.CurrentState != GameState.Playing)
        {
            SetRunAnimation(false);
            StopMovement();
            return;
        }

        if (_enemyData == null)
        {
            SetRunAnimation(false);
            StopMovement();
            return;
        }

        if (_target == null)
        {
            SetRunAnimation(false);
            StopMovement();
            return;
        }

        float distance = Vector2.Distance(_rb.position, _target.position);

        if (!CanEngage(distance))
        {
            SetRunAnimation(false);
            StopMovement();
            return;
        }

        TickCombat(distance);
    }

    protected virtual void FixedUpdate()
    {
        if (IsDead || _enemyData == null)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        if (_hasMoveDirection == false)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 finalDirection = _moveDirection + CalculateSeparation();

        if (finalDirection.sqrMagnitude > 0.0001f)
        {
            finalDirection.Normalize();
        }

        _rb.linearVelocity = finalDirection * _enemyData.MoveSpeed; // 보스 Charge 공격 중에 덮어씌워지는 문제
    }

    protected virtual bool CanEngage(float distance)
    {
        if (_enemyData == null)
            return false;

        if (distance > _enemyData.DetectionRange)
            return false;

        return HasLineOfSight(distance);
    }

    protected bool HasLineOfSight(float distance)
    {
        if (_enemyData == null || _enemyData.ObstacleMask == 0)
            return true;

        Vector2 origin = _rb.position;
        Vector2 direction = ((Vector2)_target.position - origin).normalized;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, _enemyData.ObstacleMask);

        return hit.collider == null;
    }

    protected void MoveTowardsTarget()
    {
        if (_target == null)
        {
            StopMovement();
            return;
        }

        Vector2 direction = ((Vector2)_target.position - _rb.position).normalized;
        SetFacingDirection(direction);
        SetMoveDirection(direction);
    }

    protected void MoveAwayFromTarget()
    {
        if (_target == null)
        {
            StopMovement();
            return;
        }

        Vector2 direction = (_rb.position - (Vector2)_target.position).normalized;
        SetFacingDirection(-direction);
        SetMoveDirection(direction);
    }

    protected void StopMovement()
    {
        _hasMoveDirection = false;
        _moveDirection = Vector2.zero;
        _rb.linearVelocity = Vector2.zero;
    }

    protected void SetRunAnimation(bool shouldPlay)
    {
        _shouldPlayRunAnimation = shouldPlay;
    }

    protected void SetFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        _facingDirection = direction.normalized;
    }

    protected bool CanStartAttack()
    {
        if (_enemyData == null)
            return false;

        return _isAttacking == false && Time.time >= _nextAttackTime;
    }

    protected void StartAttack()
    {
        if (CanStartAttack() == false)
            return;

        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
        }

        _attackRoutine = StartCoroutine(CoAttack());
    }

    protected abstract void TickCombat(float distance);
    protected abstract IEnumerator AttackRoutine();

    public override void HandleDeath()
    {
        if (EnterDeathState() == false)
            return;

        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        SetRunAnimation(false);
        StopMovement();

        if (_bodyCollider != null)
            _bodyCollider.enabled = false;

        _deathRoutine = StartCoroutine(CoDeath());
    }

    private IEnumerator CoAttack()
    {
        _isAttacking = true;
        StopMovement();

        yield return AttackRoutine();

        _nextAttackTime = Time.time + (_enemyData != null ? _enemyData.AttackCooldown : 0f);
        _isAttacking = false;
        _attackRoutine = null;
    }

    private IEnumerator CoDeath()
    {
        float delay = _enemyData != null ? _enemyData.DeathDespawnDelay : 0f;
        yield return new WaitForSeconds(delay);
        OnDeathFinished?.Invoke(this);
        _deathRoutine = null;
    }

    private void SetMoveDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            StopMovement();
            return;
        }

        _hasMoveDirection = true;
        _moveDirection = direction.normalized;
    }

    private Vector2 CalculateSeparation()
    {
        if (_enemyData == null)
            return Vector2.zero;

        if (_enemyData.EnemyMask == 0 || _enemyData.SeparationRadius <= 0f || _enemyData.SeparationWeight <= 0f)
            return Vector2.zero;

        Collider2D[] hits = Physics2D.OverlapCircleAll(_rb.position, _enemyData.SeparationRadius, _enemyData.EnemyMask);
        if (hits == null || hits.Length == 0)
            return Vector2.zero;

        Vector2 push = Vector2.zero;
        int count = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];

            if (hit == null || hit.attachedRigidbody == null)
                continue;

            if (hit.attachedRigidbody == _rb)
                continue;

            Vector2 diff = _rb.position - hit.attachedRigidbody.position;
            float distance = diff.magnitude;

            if (distance <= 0.0001f || distance > _enemyData.SeparationRadius)
                continue;

            float weight = 1f - (distance / _enemyData.SeparationRadius);
            push += diff.normalized * weight;
            count++;
        }

        if (count == 0)
            return Vector2.zero;

        push /= count;
        return push * _enemyData.SeparationWeight;
    }
}
