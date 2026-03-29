using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 전용 체력 컴포넌트
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int _maxHp = 5;
    [SerializeField] private int _currentHp;

    [Header("Invincibility")]
    [SerializeField] private float _invincibleDuration = 1.5f;
    private bool _isInvincible;
    private HapticManager _hapticManager;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color _hitColor = Color.red;
    [SerializeField] private float _hitFlashDuration = 0.1f;
    [SerializeField] private float _blinkInterval = 0.1f;

    private PlayerController _controller;
    private Coroutine _invincibleRoutine;
    private Coroutine _visualRoutine;
    private Color _originalColor;
    private bool _isDead;

    public int CurrentHp => _currentHp;
    public int MaxHp => _maxHp;
    public bool IsInvincible => _isInvincible;
    public bool IsDead => _isDead;

    public event Action<int> OnHit;
    public event Action<int> OnHeal;
    public event Action OnDeathStarted;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (ManagerRegistry.TryGet(out HapticManager hapticManager))
            _hapticManager = hapticManager;

        _currentHp = Mathf.Clamp(_maxHp, 1, _maxHp);

        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
    }

    public void Initialize(int maxHp)
    {
        _maxHp = Mathf.Max(1, maxHp);
        _currentHp = _maxHp;
        _isInvincible = false;
        _isDead = false;
        StopVisual();
    }

    public void TakeDamage(int damage)
    {
        if (_isDead)
            return;

        if (damage <= 0)
            return;

        if (_isInvincible)
            return;

        if (_controller != null && _controller.IsInvincible)
            return;

        if (_currentHp <= 0)
            return;

        _currentHp = Mathf.Max(0, _currentHp - damage);

        OnHit?.Invoke(damage);
        _hapticManager?.PlayPlayerHit();

        if (_currentHp <= 0)
        {
            HandleDeath();
            return;
        }

        StartInvincible();
    }

    public void Heal(int amount = 1)
    {
        if (_isDead)
            return;

        if (amount <= 0)
            return;

        int nextHp = Mathf.Min(_maxHp, _currentHp + amount);
        int actualAmount = nextHp - _currentHp;

        if (actualAmount <= 0)
            return;

        _currentHp = nextHp;
        OnHeal?.Invoke(actualAmount);
    }

    public void ResetHp()
    {
        StopInvincible();
        StopVisual();

        int restored = _maxHp - _currentHp;
        _currentHp = _maxHp;
        _isDead = false;

        if (restored > 0)
            OnHeal?.Invoke(restored);
    }

    public void HandleDeath()
    {
        if (_isDead)
            return;

        _isDead = true;
        StopInvincible();
        StopVisual();
        OnDeathStarted?.Invoke();
    }

    private void StartInvincible()
    {
        StopInvincible();
        _invincibleRoutine = StartCoroutine(InvincibleRoutine());

        if (_spriteRenderer != null)
            _visualRoutine = StartCoroutine(HitVisualRoutine());
    }

    private void StopInvincible()
    {
        _isInvincible = false;

        if (_invincibleRoutine != null)
        {
            StopCoroutine(_invincibleRoutine);
            _invincibleRoutine = null;
        }

        if (_visualRoutine != null)
        {
            StopCoroutine(_visualRoutine);
            _visualRoutine = null;
        }
    }

    private IEnumerator InvincibleRoutine()
    {
        _isInvincible = true;
        yield return new WaitForSeconds(_invincibleDuration);
        _isInvincible = false;
        _invincibleRoutine = null;
    }

    private IEnumerator HitVisualRoutine()
    {
        _spriteRenderer.DOKill();
        _spriteRenderer.color = _hitColor;
        _spriteRenderer.DOColor(_originalColor, _hitFlashDuration);

        yield return new WaitForSeconds(_hitFlashDuration);

        while (_isInvincible)
        {
            _spriteRenderer.DOFade(0f, _blinkInterval);
            yield return new WaitForSeconds(_blinkInterval);

            _spriteRenderer.DOFade(1f, _blinkInterval);
            yield return new WaitForSeconds(_blinkInterval);
        }

        _spriteRenderer.DOKill();
        _spriteRenderer.color = _originalColor;
        _spriteRenderer.DOFade(1f, 0f);
        _visualRoutine = null;
    }

    private void StopVisual()
    {
        if (_spriteRenderer == null)
            return;

        if (_visualRoutine != null)
        {
            StopCoroutine(_visualRoutine);
            _visualRoutine = null;
        }

        _spriteRenderer.DOKill();
        _spriteRenderer.color = _originalColor;
        _spriteRenderer.DOFade(1f, 0f);
    }
}