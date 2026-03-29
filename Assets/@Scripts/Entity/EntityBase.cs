using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EntityBase : MonoBehaviour, IDamageable
{
    protected Rigidbody2D _rb;

    public int CurrentHp { get; private set; }
    public int MaxHp => GetMaxHp();
    public bool IsDead { get; private set; }

    public event Action<int> OnDamaged;
    public event Action OnDied;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void OnEnable()
    {
        ResetEntityState();
    }

    protected abstract int GetMaxHp();

    protected virtual void ResetEntityState()
    {
        CurrentHp = Mathf.Max(1, GetMaxHp());
        IsDead = false;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        if (damage <= 0)
            return;

        CurrentHp = Mathf.Max(0, CurrentHp - damage);
        OnDamaged?.Invoke(damage);
        HandleDamaged(damage);

        if (CurrentHp == 0)
        {
            Die();
        }
    }

    protected virtual void HandleDamaged(int damage)
    {
    }

    protected bool EnterDeathState()
    {
        if (IsDead)
            return false;

        IsDead = true;
        OnDied?.Invoke();
        return true;
    }

    public virtual void Die()
    {
        EnterDeathState();
    }
}
