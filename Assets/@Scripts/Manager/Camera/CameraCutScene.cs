using UnityEngine;

public class CameraCutScene : MonoBehaviour
{
    [SerializeField] private GameObject _boss;
    [SerializeField] private float _bossCamSize;

    public Vector3 BossPos => _boss.transform.position;
    public float BossCamSize => _bossCamSize;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            CameraManager.Instance.BossIntroCutScene(this);
        }
    }
}
