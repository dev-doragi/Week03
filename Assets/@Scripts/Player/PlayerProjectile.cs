using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _lifetime = 1f;

    private Rigidbody2D _rb;
    private PoolManager _poolManager;
    private Coroutine _lifeRoutine;
    private bool _isInitialized;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        ManagerRegistry.TryGet(out _poolManager);
    }

    private void OnEnable()
    {
        if (_lifeRoutine != null)
        {
            StopCoroutine(_lifeRoutine);
            _lifeRoutine = null;
        }

        if (_isInitialized)
        {
            _lifeRoutine = StartCoroutine(LifeRoutine());
        }
    }

    private void OnDisable()
    {
        if (_lifeRoutine != null)
        {
            StopCoroutine(_lifeRoutine);
            _lifeRoutine = null;
        }

        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }

        _isInitialized = false;
    }

    public void Initialize(Vector2 direction, float speed, float lifetime, int damage)
    {
        _damage = damage;
        _lifetime = lifetime;
        _isInitialized = true;

        if (_rb != null)
        {
            _rb.linearVelocity = direction.normalized * speed;
            _rb.angularVelocity = 0f;
        }

        if (_lifeRoutine != null)
        {
            StopCoroutine(_lifeRoutine);
        }

        _lifeRoutine = StartCoroutine(LifeRoutine());
    }

    private IEnumerator LifeRoutine()
    {
        yield return new WaitForSeconds(_lifetime);
        ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return;

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable == null)
            damageable = other.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
        }

        ReturnToPool();
    }

    private void ReturnToPool()
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
}