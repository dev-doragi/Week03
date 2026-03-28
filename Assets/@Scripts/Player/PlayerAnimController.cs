using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    [Header("Parameters")]
    [SerializeField] private string _isMovingParam = "IsMoving";
    [SerializeField] private string _isDashingParam = "IsDashing";
    [SerializeField] private string _deathTriggerParam = "Dead";

    private PlayerController _controller;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }
    private void OnEnable()
    {
        _controller.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        _controller.OnDied -= HandleDied;
    }

    private void Update()
    {
        if (_animator == null)
            return;

        _animator.SetBool(_isMovingParam, _controller.IsMoving);
        _animator.SetBool(_isDashingParam, _controller.IsDashing);
    }

    private void HandleDied()
    {
        if (_animator == null)
            return;

        _animator.SetBool(_isMovingParam, false);
        _animator.SetBool(_isDashingParam, false);
        _animator.ResetTrigger(_deathTriggerParam);
        _animator.SetTrigger(_deathTriggerParam);
    }
}