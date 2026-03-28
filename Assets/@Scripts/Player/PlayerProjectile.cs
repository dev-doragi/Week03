using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] private int _damage = 1;

    private Rigidbody2D _rb;
    private Vector2 _direction;
    private float _speed;

    public int Damage => _damage;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, float speed, float lifetime, int damage)
    {
        _direction = direction.normalized;
        _speed = speed;
        _damage = damage;

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _direction * _speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}