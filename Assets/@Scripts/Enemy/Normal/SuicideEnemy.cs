using System.Collections;
using UnityEngine;

public class SuicideEnemy : NormalEnemyBase
{
    [SerializeField] private int _explosionDamage = 30;
    [SerializeField] private float _explosionRange = 2f;
    [SerializeField] private float _explosionWindupTime = 3f;
    [SerializeField] private float _explosionMoveSpeed = 10f;
    [SerializeField] private ParticleSystem _explosionParticle;

    private bool _isExploding = false;

    protected override void Start()
    {
        base.Start();
    }

    // Jaein 추가
    protected override void OnEnable()
    {
        base.OnEnable();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_spriteRenderer != null)
            _spriteRenderer.color = Color.white;

        _isExploding = false;
    }

    protected override void Update()
    {
        if (_isDead) return;

        if (_isExploding)
        {
            if (DetectPlayer())
                MoveToward(_player.position);
            return;
        }

        base.Update();
    }

    protected override void MoveToward(Vector2 target)
    {
        float speed = _isExploding ? _explosionMoveSpeed : _moveSpeed;
        Vector2 dir = (target - (Vector2)transform.position).normalized;

        if (_isFlying)
            _rb.linearVelocity = dir * speed;
        else
            _rb.linearVelocity = new Vector2(dir.x * speed, _rb.linearVelocity.y);
    }

    protected override IEnumerator AttackRoutine()
    {
        _canAttack = false;
        DoAttack();
        yield break;
    }

    protected override void DoAttack()
    {
        // Jaein 추가
        StopCoroutine(nameof(ExplosionRoutine));
        StartCoroutine(nameof(ExplosionRoutine));
    }

    IEnumerator ExplosionRoutine()
    {
        _isExploding = true;

        float elapsed = 0f;
        float blinkInterval = 0.3f;

        while (elapsed < _explosionWindupTime)
        {
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(blinkInterval);
            _spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval * 2;
            blinkInterval = Mathf.Max(0.05f, blinkInterval - 0.03f);
        }

        Explode();
    }

    void Explode()
    {
        if (_explosionParticle != null)
        {
            ParticleSystem particle = Instantiate(_explosionParticle, transform.position, Quaternion.identity);
            particle.transform.localScale = Vector3.one * _explosionRange;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _explosionRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
                hit.GetComponent<IDamageable>()?.TakeDamage(_explosionDamage);
        }

        Die();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;
        StopAllCoroutines();
        Explode();
    }

    protected override IEnumerator OnDieRoutine()
    {
        yield return new WaitForSeconds(0.3f);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _explosionRange);
    }
}