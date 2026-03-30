using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public abstract class ProjectileBase : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private LayerMask _hitMask;
    [SerializeField] private LayerMask _wallMask;

    private PoolManager _poolManager;
    private Coroutine _lifeRoutine;

    private Vector2 _direction = Vector2.right;
    private float _speed;
    private float _lifetime;
    private int _damage;
    private GameObject _owner;
    private bool _isInitialized;

    protected virtual void Awake()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }

        ManagerRegistry.TryGet(out _poolManager);
    }

    protected virtual void OnEnable()
    {
        StopLifeRoutine();
        ResetMotionState();
    }

    protected virtual void OnDisable()
    {
        StopLifeRoutine();
        ResetMotionState();
        ResetRuntimeState();
    }

    protected void InitializeProjectile(Vector2 direction, float speed, float lifetime, int damage, GameObject owner)
    {
        _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        _speed = speed;
        _lifetime = lifetime;
        _damage = damage;
        _owner = owner;
        _isInitialized = true;

        StopLifeRoutine();

        if (_lifetime > 0f)
            _lifeRoutine = StartCoroutine(LifeRoutine());
    }

    private IEnumerator LifeRoutine()
    {
        yield return new WaitForSeconds(_lifetime);
        ReturnToPool();
    }

    private void FixedUpdate()
    {
        if (!_isInitialized || _rb == null)
            return;

        Vector2 nextPosition = _rb.position + (_direction * _speed * Time.fixedDeltaTime);
        _rb.MovePosition(nextPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
            return;

        // 1) 인스펙터로 지정된 _wallMask에 포함되면 즉시 풀로 반환
        if (_wallMask != 0 && (_wallMask.value & (1 << other.gameObject.layer)) != 0)
        {
            ReturnToPool();
            return;
        }

        // 2) 프로젝트 전역의 "Wall" 레이어 이름이 설정되어 있다면 폴백으로 처리
        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer >= 0 && other.gameObject.layer == wallLayer)
        {
            ReturnToPool();
            return;
        }

        if (!CanProcessCollision(other))
            return;

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = other.GetComponentInParent<IDamageable>();

        if (damageable != null)
            damageable.TakeDamage(_damage);

        ReturnToPool();
    }

    protected virtual bool CanProcessCollision(Collider2D other)
    {
        if (!_isInitialized || other == null)
            return false;

        if (_owner != null && other.gameObject == _owner)
            return false;

        if (_hitMask != 0 && (_hitMask.value & (1 << other.gameObject.layer)) == 0)
            return false;

        return true;
    }

    protected void ReturnToPool()
    {
        if (!gameObject.activeSelf)
            return;

        if (_poolManager != null)
        {
            _poolManager.Return(gameObject);
            return;
        }

        gameObject.SetActive(false);
    }

    private void StopLifeRoutine()
    {
        if (_lifeRoutine == null)
            return;

        StopCoroutine(_lifeRoutine);
        _lifeRoutine = null;
    }

    private void ResetMotionState()
    {
        if (_rb == null)
            return;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
    }

    private void ResetRuntimeState()
    {
        _direction = Vector2.right;
        _speed = 0f;
        _lifetime = 0f;
        _damage = 0;
        _owner = null;
        _isInitialized = false;
    }
}