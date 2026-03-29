using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private LayerMask _hitMask;

    private Vector2 _direction;
    private float _speed;
    private float _lifeTime;
    private int _damage;
    private GameObject _owner;
    private float _spawnTime;

    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public void Initialize(Vector2 direction, float speed, int damage, float lifeTime, GameObject owner)
    {
        _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        _speed = speed;
        _damage = damage;
        _lifeTime = lifeTime;
        _owner = owner;
        _spawnTime = Time.time;
    }

    private void FixedUpdate()
    {
        Vector2 delta = _direction * _speed * Time.fixedDeltaTime;

        if (_rb != null)
        {
            _rb.MovePosition(_rb.position + delta);
        }
        else
        {
            transform.position += (Vector3)delta;
        }

        if (_lifeTime > 0f && Time.time - _spawnTime >= _lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
            return;

        if (other.gameObject == _owner)
            return;

        if (_hitMask != 0 && (_hitMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(_damage);
        }

        Destroy(gameObject);
    }
}