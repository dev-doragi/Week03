using UnityEngine;

public class CameraSize : MonoBehaviour
{
    [SerializeField] float _cameraSize =15f;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        CameraManager.Instance.SetSize(_cameraSize);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        CameraManager.Instance.ResetSize();
    }
}
