using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAim : MonoBehaviour
{
    private const float MIN_AIM_SQR = 0.0001f;

    [Header("References")]
    [SerializeField] private Transform _gunPivot;

    [Header("Options")]
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private float _gamepadDeadZone = 0.2f;

    private PlayerController _controller;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    private void Start()
    {
        if (_targetCamera == null)
            _targetCamera = Camera.main;
    }

    public void UpdateAim()
    {
        if (_gunPivot == null)
            return;

        Vector2 nextAimDirection = _controller.AimDirection;

        if (_controller.IsGamepadInput)
        {
            Vector2 lookInput = _controller.LookInput;
            float deadZoneSqr = _gamepadDeadZone * _gamepadDeadZone;

            if (lookInput.sqrMagnitude > deadZoneSqr)
                nextAimDirection = lookInput.normalized;
        }
        else
        {
            if (_targetCamera == null)
                return;

            Vector3 screenPosition = new Vector3(
                _controller.LookInput.x,
                _controller.LookInput.y,
                Mathf.Abs(_targetCamera.transform.position.z));

            Vector3 worldPosition = _targetCamera.ScreenToWorldPoint(screenPosition);
            Vector2 direction = worldPosition - _gunPivot.position;

            if (direction.sqrMagnitude > MIN_AIM_SQR)
                nextAimDirection = direction.normalized;
        }

        if (nextAimDirection.sqrMagnitude <= MIN_AIM_SQR)
            return;

        _controller.SetAimDirection(nextAimDirection);

        float angle = Mathf.Atan2(nextAimDirection.y, nextAimDirection.x) * Mathf.Rad2Deg;
        _gunPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}