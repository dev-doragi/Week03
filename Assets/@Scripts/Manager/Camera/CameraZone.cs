using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [SerializeField] private Vector3 _offset;
    [SerializeField] private bool _useBounds;
    [SerializeField] private Vector2 _minBounds;
    [SerializeField] private Vector2 _maxBounds;
    [SerializeField] private float _correctionX;
    [SerializeField] private float _correctionY;

    public Vector3 Offset => _offset;
    public bool UseBounds => _useBounds;
    public Vector2 MinBounds => _minBounds;
    public Vector2 MaxBounds => _maxBounds;
    public float CorrectionX => _correctionX;
    public float CorrectionY => _correctionY;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        CameraManager.Instance.SetZone(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        CameraManager.Instance.ClearZone(this);
    }
}
