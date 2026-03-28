using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SpawnTrigger : MonoBehaviour
{
    [SerializeField] private EnemySpawner _enemySpawner;

    private BoxCollider2D _boxCollider;
    private bool _hasTriggered;

    public void Initialize(EnemySpawner enemySpawner)
    {
        _enemySpawner = enemySpawner;
    }

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (_enemySpawner == null)
            return;

        _hasTriggered = true;
        _enemySpawner.StartFirstWave();
    }
}