using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerMovement : MonoBehaviour
{
    private const float MIN_INPUT_SQR = 0.0001f;

    [SerializeField] private PlayerMovementSO _movementData;

    private PlayerController _controller;
    private Coroutine _dashRoutine;
    private float _lastDashTime = -100f;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    public void HandleMove()
    {
        if (_movementData == null)
            return;

        if (_controller.IsDashing)
            return;

        _controller.Rigidbody.linearVelocity = _controller.MoveInput * _movementData.MoveSpeed;
    }

    public void HandleDash()
    {
        if (_movementData == null)
            return;

        if (_controller.IsDashing)
            return;

        if (!_controller.IsDashPressedThisFrame)
            return;

        if (_controller.MoveInput.sqrMagnitude <= MIN_INPUT_SQR)
            return;

        if (Time.time < _lastDashTime + _movementData.DashCooldown)
            return;

        Vector2 dashDirection = GetDashDirection();
        if (dashDirection.sqrMagnitude <= MIN_INPUT_SQR)
            return;

        if (_dashRoutine != null)
            StopCoroutine(_dashRoutine);

        _dashRoutine = StartCoroutine(DashRoutine(dashDirection));
    }

    private Vector2 GetDashDirection()
    {
        if (_controller.MoveInput.sqrMagnitude > MIN_INPUT_SQR)
            return _controller.MoveInput.normalized;

        return _controller.AimDirection;
    }

    public void StopMovementImmediate()
    {
        if (_dashRoutine != null)
        {
            StopCoroutine(_dashRoutine);
            _dashRoutine = null;
        }

        _controller.SetDashing(false);
        _controller.SetInvincible(false);
        _controller.Rigidbody.linearVelocity = Vector2.zero;
    }

    private IEnumerator DashRoutine(Vector2 dashDirection)
    {
        _lastDashTime = Time.time;
        _controller.SetDashing(true);
        _controller.SetInvincible(_movementData.DashInvincible);

        float elapsed = 0f;
        while (elapsed < _movementData.DashDuration)
        {
            _controller.Rigidbody.linearVelocity = dashDirection * _movementData.DashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        _controller.Rigidbody.linearVelocity = Vector2.zero;
        _controller.SetInvincible(false);
        _controller.SetDashing(false);
        _dashRoutine = null;
    }
}