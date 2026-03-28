using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollowTarget : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _maxOffsetDistance = 3f;
    [SerializeField] private float _offsetSmoothSpeed = 8f;

    private Vector3 _currentOffset;

    private void Awake()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_player == null || _mainCamera == null)
            return;

        if (Mouse.current == null)
        {
            transform.position = _player.position + _currentOffset;
            return;
        }

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, 0f));

        mouseWorldPosition.z = 0f;

        Vector3 playerPosition = _player.position;
        Vector3 aimDirection = mouseWorldPosition - playerPosition;
        aimDirection.z = 0f;

        Vector3 targetOffset = Vector3.zero;

        if (aimDirection.sqrMagnitude > 0.001f)
            targetOffset = aimDirection.normalized * _maxOffsetDistance;

        _currentOffset = Vector3.Lerp(
            _currentOffset,
            targetOffset,
            _offsetSmoothSpeed * Time.deltaTime);

        transform.position = playerPosition + _currentOffset;
    }
}