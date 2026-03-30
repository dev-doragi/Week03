using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerAnimController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    [Header("Parameters")]
    [SerializeField] private string _isMovingParam = "IsMoving";
    [SerializeField] private string _isDashingParam = "IsDashing";
    [SerializeField] private string _isBackwardMoveParam = "IsBackwardMove";
    [SerializeField] private string _deathTriggerParam = "Dead";

    [Header("Backward Move")]
    [SerializeField] private float _backwardThreshold = -0.1f;

    private PlayerController _controller;
    private PlayerHealth _playerHealth;

    // WeaponPivot 캐싱용 변수
    private GameObject _weaponPivot;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _playerHealth = GetComponent<PlayerHealth>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        // 비활성화된 자식까지 포함하여 모든 트랜스폼을 검사해 WeaponPivot을 찾습니다.
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < allChildren.Length; i++)
        {
            if (allChildren[i].name == "WeaponPivot")
            {
                _weaponPivot = allChildren[i].gameObject;
                break;
            }
        }
    }

    private void OnEnable()
    {
        if (_playerHealth != null)
            _playerHealth.OnDeathStarted += HandleDeathStarted;
    }

    private void OnDisable()
    {
        if (_playerHealth != null)
            _playerHealth.OnDeathStarted -= HandleDeathStarted;
    }

    private void Update()
    {
        if (_animator == null)
            return;

        if (_playerHealth != null && _playerHealth.IsDead)
            return;

        bool isMoving = _controller.IsMoving;
        bool isDashing = _controller.IsDashing;
        bool isBackwardMove = false;

        Vector2 moveInput = _controller.MoveInput;
        Vector2 aimDirection = _controller.AimDirection;

        if (isMoving && !isDashing && moveInput.sqrMagnitude > 0.001f && aimDirection.sqrMagnitude > 0.001f)
        {
            float dot = Vector2.Dot(moveInput.normalized, aimDirection.normalized);
            isBackwardMove = dot < _backwardThreshold;
        }

        _animator.SetBool(_isMovingParam, isMoving);
        _animator.SetBool(_isDashingParam, isDashing);
        _animator.SetBool(_isBackwardMoveParam, isBackwardMove);
    }

    private void HandleDeathStarted()
    {
        // 사망 시 WeaponPivot 비활성화
        if (_weaponPivot != null)
            _weaponPivot.SetActive(false);

        if (_animator == null)
            return;

        _animator.SetBool(_isMovingParam, false);
        _animator.SetBool(_isDashingParam, false);
        _animator.SetBool(_isBackwardMoveParam, false);
        _animator.ResetTrigger(_deathTriggerParam);
        _animator.SetTrigger(_deathTriggerParam);
    }

    public void ResetAnimationState()
    {
        // 재시작(초기화) 시 WeaponPivot 활성화
        if (_weaponPivot != null)
            _weaponPivot.SetActive(true);

        if (_animator == null)
            return;

        _animator.Rebind();
        _animator.Update(0f);
        _animator.SetBool(_isMovingParam, false);
        _animator.SetBool(_isDashingParam, false);
        _animator.SetBool(_isBackwardMoveParam, false);
        _animator.ResetTrigger(_deathTriggerParam);
    }
}