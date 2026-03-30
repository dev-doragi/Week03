using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class InteractionProp : MonoBehaviour, IInteractable
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _outlineSprite;
    [SerializeField] private Transform _uiPivot;

    [Header("Throw")]
    [SerializeField] private float _throwSpeed = 20f;
    [SerializeField] private float _lifeTime = 3f;
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _ownerCollisionIgnoreDuration = 0.2f;
    [SerializeField] private float _spawnForwardOffset = 0.6f;

    [Header("Temporary Damping")]
    [SerializeField] private float _thrownLinearDamping = 1f;
    [SerializeField] private float _thrownAngularDamping = 1f;
    [SerializeField] private float _temporaryDampingDuration = 0.2f;

    [Header("Auto Target")]
    [SerializeField] private float _autoTargetRange = 8f;
    [SerializeField] private LayerMask _enemyLayer;

    //[Header("Hit Feedback")]
    //[SerializeField] private float _enemyHitShakeDuration = 0.08f;
    //[SerializeField] private float _enemyHitShakeStrength = 1.2f;

    private Rigidbody2D _rb;
    private Collider2D _collider;

    private bool _isThrown;
    private bool _canInteract = true;
    private float _remainingLifeTime;
    private GameObject _owner;

    private float _defaultLinearDamping;
    private float _defaultAngularDamping;

    private Coroutine _restoreDampingRoutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        _defaultLinearDamping = _rb.linearDamping;
        _defaultAngularDamping = _rb.angularDamping;

        _rb.gravityScale = 0f;
        _rb.simulated = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
    }

    private void Update()
    {
        if (_isThrown == false)
            return;

        _remainingLifeTime -= Time.deltaTime;
        if (_remainingLifeTime <= 0f)
            StopThrownState();
    }

    public void Interact(GameObject interactor)
    {
        if (_canInteract == false)
            return;

        if (_isThrown)
            return;

        if (interactor == null)
            return;

        PlayerController playerController = interactor.GetComponent<PlayerController>();
        if (playerController == null)
            return;

        Vector2 throwDirection = GetDirectionToNearestEnemy();

        if (throwDirection.sqrMagnitude <= 0.001f)
        {
            throwDirection = playerController.AimDirection;
            if (throwDirection.sqrMagnitude <= 0.001f)
                throwDirection = playerController.IsFacingLeft ? Vector2.left : Vector2.right;
        }

        Throw(interactor, throwDirection.normalized);
    }

    public string GetInteractionText()
    {
        if (_canInteract == false)
            return string.Empty;

        return "던지기";
    }

    public void ShowOutline(bool show)
    {
        if (_canInteract == false)
            show = false;

        if (_renderer == null)
            return;

        if (_outlineSprite == null || _normalSprite == null)
            return;

        _renderer.sprite = show ? _outlineSprite : _normalSprite;
    }

    public Transform GetUIPivot()
    {
        return _uiPivot != null ? _uiPivot : transform;
    }

    private void Throw(GameObject owner, Vector2 direction)
    {
        _isThrown = true;
        _canInteract = false;
        _owner = owner;
        _remainingLifeTime = _lifeTime;

        ShowOutline(false);
        transform.position += (Vector3)(direction * _spawnForwardOffset);

        IgnoreOwnerCollisionTemporarily(owner);
        ApplyTemporaryDamping();

        _rb.angularVelocity = 0f;
        _rb.linearVelocity = direction * _throwSpeed;
    }

    private Vector2 GetDirectionToNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _autoTargetRange, _enemyLayer);

        Transform nearestTarget = null;
        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null)
                continue;

            Component damageableComponent = hit.GetComponentInParent(typeof(IDamageable)) as Component;
            if (damageableComponent == null)
                continue;

            Transform targetTransform = damageableComponent.transform;
            Vector2 toTarget = (Vector2)targetTransform.position - (Vector2)transform.position;
            float distanceSqr = toTarget.sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestTarget = targetTransform;
            }
        }

        if (nearestTarget == null)
            return Vector2.zero;

        return ((Vector2)nearestTarget.position - (Vector2)transform.position).normalized;
    }

    private void ApplyTemporaryDamping()
    {
        _rb.linearDamping = _thrownLinearDamping;
        _rb.angularDamping = _thrownAngularDamping;

        if (_restoreDampingRoutine != null)
            StopCoroutine(_restoreDampingRoutine);

        _restoreDampingRoutine = StartCoroutine(CoRestoreDamping());
    }

    private IEnumerator CoRestoreDamping()
    {
        yield return new WaitForSeconds(_temporaryDampingDuration);

        _rb.linearDamping = _defaultLinearDamping;
        _rb.angularDamping = _defaultAngularDamping;
        _restoreDampingRoutine = null;
    }

    private void IgnoreOwnerCollisionTemporarily(GameObject owner)
    {
        Collider2D[] ownerColliders = owner.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < ownerColliders.Length; i++)
        {
            if (ownerColliders[i] == null)
                continue;

            Physics2D.IgnoreCollision(_collider, ownerColliders[i], true);
        }

        StartCoroutine(CoRestoreOwnerCollision(ownerColliders));
    }

    private IEnumerator CoRestoreOwnerCollision(Collider2D[] ownerColliders)
    {
        yield return new WaitForSeconds(_ownerCollisionIgnoreDuration);

        for (int i = 0; i < ownerColliders.Length; i++)
        {
            if (ownerColliders[i] == null)
                continue;

            Physics2D.IgnoreCollision(_collider, ownerColliders[i], false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isThrown == false)
            return;

        GameObject hitObject = collision.collider.gameObject;

        if (_owner != null && hitObject.transform.root.gameObject == _owner.transform.root.gameObject)
            return;

        IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
            CameraShakeModule.Instance?.Play(0.12f, 3f);
        }

        StopThrownState();
    }

    private void StopThrownState()
    {
        _isThrown = false;
        _remainingLifeTime = 0f;
        _owner = null;

        if (_restoreDampingRoutine != null)
        {
            StopCoroutine(_restoreDampingRoutine);
            _restoreDampingRoutine = null;
        }

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.linearDamping = _defaultLinearDamping;
        _rb.angularDamping = _defaultAngularDamping;
    }
}