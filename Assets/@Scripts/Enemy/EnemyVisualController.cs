using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyVisualController : MonoBehaviour
{
    [SerializeField] private EnemyBase _enemy;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;

    private Coroutine _hitRoutine;
    private bool _isDead;
    private Vector3 _baseScale;

    private void Awake()
    {
        if (_enemy == null)
            _enemy = GetComponentInParent<EnemyBase>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_animator == null)
            _animator = GetComponent<Animator>();

        _baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        _isDead = false;

        if (_enemy != null)
        {
            _enemy.OnDamaged += HandleDamaged;
            _enemy.OnDied += HandleDied;
        }

        ResetVisualState();
    }

    private void OnDisable()
    {
        if (_enemy != null)
        {
            _enemy.OnDamaged -= HandleDamaged;
            _enemy.OnDied -= HandleDied;
        }

        if (_hitRoutine != null)
        {
            StopCoroutine(_hitRoutine);
            _hitRoutine = null;
        }
    }

    private void LateUpdate()
    {
        if (_isDead || _enemy == null)
            return;

        UpdateAnimatorState();
        UpdateFlip();
    }

    private void ResetVisualState()
    {
        transform.localScale = _baseScale;

        if (_animator != null)
        {
            _animator.enabled = true;
            _animator.Rebind();
            _animator.Update(0f);
            _animator.speed = 0f;
        }
    }

    private void UpdateAnimatorState()
    {
        if (_animator == null)
            return;

        _animator.speed = _enemy.ShouldPlayRunAnimation ? 1f : 0f;
    }

    private void UpdateFlip()
    {
        float flipThreshold = 0.01f;

        if (_enemy.EnemyData != null)
        {
            flipThreshold = _enemy.EnemyData.FlipThreshold;
        }

        Vector2 facing = _enemy.FacingDirection;

        if (Mathf.Abs(facing.x) <= flipThreshold)
            return;

        Vector3 scale = _baseScale;
        scale.x = Mathf.Abs(scale.x) * (facing.x < 0f ? -1f : 1f);
        transform.localScale = scale;
    }

    private void HandleDamaged(int damage)
    {
        if (_isDead || _enemy == null || _enemy.EnemyData == null)
            return;

        if (_enemy.EnemyData.HitSprite == null)
            return;

        if (_hitRoutine != null)
            StopCoroutine(_hitRoutine);

        _hitRoutine = StartCoroutine(CoShowHitSprite());
    }

    private void HandleDied()
    {
        _isDead = true;

        if (_hitRoutine != null)
        {
            StopCoroutine(_hitRoutine);
            _hitRoutine = null;
        }

        if (_animator != null)
            _animator.enabled = false;

        if (_enemy != null && _enemy.EnemyData != null && _enemy.EnemyData.DeathSprite != null)
        {
            _spriteRenderer.sprite = _enemy.EnemyData.DeathSprite;
        }
    }

    private IEnumerator CoShowHitSprite()
    {
        if (_enemy == null || _enemy.EnemyData == null)
            yield break;

        if (_animator != null)
            _animator.enabled = false;

        _spriteRenderer.sprite = _enemy.EnemyData.HitSprite;
        yield return new WaitForSeconds(_enemy.EnemyData.HitSpriteDuration);

        if (_isDead == false && _animator != null)
            _animator.enabled = true;

        _hitRoutine = null;
    }
}
