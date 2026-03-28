using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerVisualController : MonoBehaviour
{
    private const float MIN_AIM_X = 0.0001f;

    [Header("References")]
    [SerializeField] private SpriteRenderer _bodyRenderer;
    [SerializeField] private Transform _gunVisualRoot;

    private PlayerController _controller;
    private Vector3 _gunVisualRootScale;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();

        if (_gunVisualRoot != null)
            _gunVisualRootScale = _gunVisualRoot.localScale;
    }

    private void LateUpdate()
    {
        UpdateVisualDirection();
    }

    private void UpdateVisualDirection()
    {
        Vector2 aimDirection = _controller.AimDirection;

        if (Mathf.Abs(aimDirection.x) <= MIN_AIM_X)
            return;

        bool isFacingLeft = aimDirection.x < 0f;
        _controller.SetFacingLeft(isFacingLeft);

        if (_bodyRenderer != null)
            _bodyRenderer.flipX = isFacingLeft;

        if (_gunVisualRoot != null)
        {
            Vector3 nextScale = _gunVisualRootScale;
            nextScale.y = isFacingLeft ? -Mathf.Abs(_gunVisualRootScale.y) : Mathf.Abs(_gunVisualRootScale.y);
            _gunVisualRoot.localScale = nextScale;
        }
    }
}