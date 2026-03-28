using UnityEngine;

public abstract class EntityBase : MonoBehaviour, IDamageable
{
    // =====================
    // 공통 스탯
    // =====================
    [SerializeField] protected int _maxHp = 5;
    [SerializeField] protected int _attackDamage = 1;

    protected int _currentHp;
    protected Rigidbody2D _rb;

    // =====================
    // 프로퍼티
    // =====================
    public int CurrentHp => _currentHp;
    public int MaxHp => _maxHp;

    // =====================
    // 생명주기
    // =====================
    protected virtual void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        Initialize();
    }

    protected virtual void Initialize()
    {
        _currentHp = _maxHp;
    }

    // =====================
    // IDamageable 구현
    // =====================
    public virtual void TakeDamage(int damage)
    {
        if (_currentHp <= 0) return;

        _currentHp -= damage;

        if (_currentHp <= 0)
        {
            _currentHp = 0;
            Die();
        }
    }

    public virtual void Die() { }
}