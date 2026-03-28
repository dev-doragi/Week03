using Unity.Cinemachine;
using UnityEngine;

public class CameraBoundsController : MonoBehaviour
{
    private bool _useBounds;
    private Vector2 _minBounds;
    private Vector2 _maxBounds;
    private float _correctionY;
    private float _correctionX;

    public void SetBounds(Vector2 min, Vector2 max, bool enabled, float correctionX, float correctionY)
    {
        _minBounds = min;
        _maxBounds = max;
        _useBounds = enabled;
        _correctionX = correctionX;
        _correctionY = correctionY;
    }

    public void ClearBounds()
    {
        _useBounds = false;
    }

    private void LateUpdate()
    {
        if (!_useBounds) return;

        Vector3 pos = transform.parent.position;
        pos.x = Mathf.Clamp(pos.x, _minBounds.x, _maxBounds.x) + _correctionX;
        pos.y = Mathf.Clamp(pos.y, _minBounds.y, _maxBounds.y) + _correctionY;
        transform.position = pos;
    }
}
