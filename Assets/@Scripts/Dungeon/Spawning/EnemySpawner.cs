using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Data")]
    [SerializeField] private List<SO_WaveData> _waveDatas = new();

    [Header("Stop Block")]
    [SerializeField] private GameObject[] _blocks;

    [Header("Spawn Points")]
    [SerializeField] private List<Transform> _spawnPoints = new();

    [Header("Options")]
    [SerializeField] private bool _autoStart = false;
    [SerializeField] private bool _stopWhenSpawnPointIsShort = true;
    [SerializeField, Range(0f, 1f)] private float _changeWaveThreshold = 0.8f;

    private int _currentWaveKilledCount = 0;
    private bool _isWaveSpawnCompleted;
    private bool _isChangingWave = false;
    private int _currentWaveIndex = -1;
    private Coroutine _spawnRoutine;
    private PoolManager _pool;
    private bool _isShuttingDown = false;
    private bool _hasStarted = false;
    private Transform _target;
    private readonly List<EnemyBase> _spawnedEnemies = new();

    public bool HasStarted => _hasStarted;

    private void Awake()
    {
        ManagerRegistry.TryGet(out _pool);
        SortWaveDatas();
    }

    private void Start()
    {
        if (_autoStart)
        {
            StartFirstWave();
        }
    }

    private void OnDestroy()
    {
        _isShuttingDown = true;
        ClearWaveRoutine();
        UnsubscribeAllEnemies();
    }

    public void SetTarget(Transform target)
    {
        _target = target;

        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            if (_spawnedEnemies[i] != null)
            {
                _spawnedEnemies[i].SetTarget(_target);
            }
        }
    }

    public void InitializeSpawnPoints(List<Transform> spawnPoints)
    {
        _spawnPoints.Clear();

        if (spawnPoints == null)
            return;

        _spawnPoints.AddRange(spawnPoints);
    }

    public int GetMaxRequiredSpawnPointCount()
    {
        int maxCount = 0;

        for (int i = 0; i < _waveDatas.Count; i++)
        {
            SO_WaveData waveData = _waveDatas[i];
            if (waveData == null)
                continue;

            if (waveData.enemyPrefabs.Count > maxCount)
            {
                maxCount = waveData.enemyPrefabs.Count;
            }
        }

        return maxCount;
    }

    public void SetBlocks(GameObject[] blocks)
    {
        _blocks = blocks;
    }

    public void StartFirstWave()
    {
        if (_hasStarted)
            return;

        _hasStarted = true;

        if (_isShuttingDown || !this)
            return;

        if (_blocks != null)
        {
            for (int i = 0; i < _blocks.Length; i++)
            {
                if (_blocks[i] != null)
                    _blocks[i].SetActive(true);
            }
        }

        if (_waveDatas == null || _waveDatas.Count == 0)
        {
            Debug.LogWarning($"{name}: WaveData가 없습니다.");
            return;
        }

        StartWave(0);
    }

    private void SortWaveDatas()
    {
        _waveDatas.Sort((a, b) => a.id.CompareTo(b.id));
    }

    public void StartWave(int waveIndex)
    {
        if (_isShuttingDown || !this || !gameObject.activeInHierarchy)
            return;

        if (waveIndex < 0 || waveIndex >= _waveDatas.Count)
        {
            Debug.LogWarning($"{name}: 잘못된 waveIndex = {waveIndex}");
            return;
        }

        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        UnsubscribeAllEnemies();

        _currentWaveIndex = waveIndex;
        _currentWaveKilledCount = 0;
        _isWaveSpawnCompleted = false;
        _isChangingWave = false;

        _spawnRoutine = StartCoroutine(CoSpawnWave(_waveDatas[_currentWaveIndex]));
    }

    private bool ShouldNextWave()
    {
        if (_isShuttingDown)
            return false;

        if (!_isWaveSpawnCompleted)
            return false;

        if (_currentWaveIndex < 0 || _currentWaveIndex >= _waveDatas.Count)
            return false;

        SO_WaveData waveData = _waveDatas[_currentWaveIndex];
        int totalCount = waveData.enemyPrefabs.Count;

        if (totalCount <= 0)
            return false;

        float threshold = waveData.isBossWave ? 1f : _changeWaveThreshold;
        int requiredKillCount = Mathf.CeilToInt(totalCount * threshold);

        return _currentWaveKilledCount >= requiredKillCount;
    }

    public void StartNextWave()
    {
        if (_isShuttingDown || !this || !gameObject.activeInHierarchy)
            return;

        int nextIndex = _currentWaveIndex + 1;

        if (nextIndex >= _waveDatas.Count)
        {
            Debug.Log($"{name}: 모든 웨이브 완료");

            for (int i = 0; i < _blocks.Length; i++)
            {
                if (_blocks[i] != null)
                {
                    _blocks[i].SetActive(false);
                }
            }

            return;
        }

        StartWave(nextIndex);
    }

    private IEnumerator CoSpawnWave(SO_WaveData waveData)
    {
        if (_isShuttingDown)
        {
            _spawnRoutine = null;
            yield break;
        }

        if (waveData == null)
        {
            _spawnRoutine = null;
            yield break;
        }

        if (_spawnPoints == null || _spawnPoints.Count == 0)
        {
            Debug.LogWarning($"{name}: SpawnPoint가 없습니다.");
            _spawnRoutine = null;
            yield break;
        }

        int enemyCount = waveData.enemyPrefabs.Count;
        int pointCount = _spawnPoints.Count;

        if (enemyCount == 0)
        {
            Debug.LogWarning($"{name}: Wave {waveData.id} 에 배치할 적이 없습니다.");
            _spawnRoutine = null;
            yield break;
        }

        if (enemyCount > pointCount)
        {
            Debug.LogWarning($"{name}: Wave {waveData.id} 적 수({enemyCount})가 스폰 위치 수({pointCount})보다 많습니다.");

            if (_stopWhenSpawnPointIsShort)
            {
                _spawnRoutine = null;
                yield break;
            }
        }

        List<Transform> selectedPoints = GetSpawnPointsForWave(enemyCount);

        if (waveData.spawnSequentially)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                if (_isShuttingDown)
                {
                    _spawnRoutine = null;
                    yield break;
                }

                SpawnEnemy(waveData.enemyPrefabs[i], selectedPoints[i]);

                if (i < enemyCount - 1)
                {
                    yield return new WaitForSeconds(waveData.spawnInterval);
                }
            }
        }
        else
        {
            for (int i = 0; i < enemyCount; i++)
            {
                if (_isShuttingDown)
                {
                    _spawnRoutine = null;
                    yield break;
                }

                SpawnEnemy(waveData.enemyPrefabs[i], selectedPoints[i]);
            }
        }

        _isWaveSpawnCompleted = true;
        _spawnRoutine = null;
    }

    private void SpawnEnemy(GameObject enemyPrefab, Transform spawnPoint)
    {
        if (_isShuttingDown)
            return;

        if (enemyPrefab == null || spawnPoint == null)
            return;

        GameObject enemyObject;

        if (_pool != null)
        {
            enemyObject = _pool.Get(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            enemyObject = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        DespawnController despawnController = enemyObject.GetComponent<DespawnController>();
        if (despawnController != null)
        {
            despawnController.SetMode(E_DespawnMode.ReturnToPool);
        }

        EnemyBase enemyBase = enemyObject.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            enemyBase.SetTarget(_target);
            enemyBase.OnDeathFinished -= HandleEnemyDeathFinished;
            enemyBase.OnDeathFinished += HandleEnemyDeathFinished;

            if (!_spawnedEnemies.Contains(enemyBase))
            {
                _spawnedEnemies.Add(enemyBase);
            }
        }
    }

    private List<Transform> GetSpawnPointsForWave(int enemyCount)
    {
        List<Transform> shuffled = new List<Transform>(_spawnPoints);

        for (int i = 0; i < shuffled.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[randomIndex]) = (shuffled[randomIndex], shuffled[i]);
        }

        if (enemyCount <= shuffled.Count)
        {
            return shuffled.GetRange(0, enemyCount);
        }

        List<Transform> result = new List<Transform>(enemyCount);

        for (int i = 0; i < enemyCount; i++)
        {
            result.Add(shuffled[i % shuffled.Count]);
        }

        return result;
    }

    private void HandleEnemyDeathFinished(EnemyBase enemy)
    {
        if (_isShuttingDown || !this || !gameObject.activeInHierarchy)
            return;

        if (enemy != null)
        {
            enemy.OnDeathFinished -= HandleEnemyDeathFinished;
            _spawnedEnemies.Remove(enemy);
        }

        if (_isChangingWave)
            return;

        _currentWaveKilledCount++;

        if (ShouldNextWave())
        {
            _isChangingWave = true;
            StartNextWave();
        }
    }

    private void UnsubscribeAllEnemies()
    {
        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            if (_spawnedEnemies[i] != null)
            {
                _spawnedEnemies[i].OnDeathFinished -= HandleEnemyDeathFinished;
            }
        }

        _spawnedEnemies.Clear();
    }

    public void ClearWaveRoutine()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }
}
