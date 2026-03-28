using UnityEngine;

public class DespawnController : MonoBehaviour
{
    [SerializeField] private E_DespawnMode _mode = E_DespawnMode.Destroy;

    private EnemyBase _enemy;
    private PoolManager _pool;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        ManagerRegistry.TryGet(out _pool);
    }

    private void OnEnable()
    {
        if (_enemy != null)
        {
            _enemy.OnDeathFinished += HandleDeathFinished;
        }
    }

    private void OnDisable()
    {
        if (_enemy != null)
        {
            _enemy.OnDeathFinished -= HandleDeathFinished;
        }
    }

    public void SetMode(E_DespawnMode mode)
    {
        _mode = mode;
    }

    private void HandleDeathFinished(EnemyBase enemy)
    {
        switch (_mode)
        {
            case E_DespawnMode.Disable:
                gameObject.SetActive(false);
                break;

            case E_DespawnMode.Destroy:
                Destroy(gameObject);
                break;

            case E_DespawnMode.ReturnToPool:
                if (_pool != null)
                {
                    _pool.Return(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
                break;
        }
    }
}