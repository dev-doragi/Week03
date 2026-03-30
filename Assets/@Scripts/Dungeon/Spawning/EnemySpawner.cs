using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Data")]
    [SerializeField] private List<SO_WaveData> _waveDatas = new();

    [Header("Boss Room")]
    [SerializeField] private bool _isBossRoom = false;
    [SerializeField] private EnemyBase _bossPrefab;
    [SerializeField] private Transform _bossSpawnPoint;
    [SerializeField] private float _bossIntroDelay = 1.5f;
    [SerializeField] private float _reinforcementInitialDelay = 5f;
    [SerializeField] private float _reinforcementInterval = 12f;

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
    private Coroutine _reinforcementRoutine;
    private Coroutine _bossIntroRoutine;
    private PoolManager _pool;
    private bool _isShuttingDown = false;
    private bool _hasStarted = false;
    private Transform _target;
    private EnemyBase _spawnedBoss;
    private readonly List<EnemyBase> _spawnedEnemies = new();

    public bool HasStarted => _hasStarted;

    public event Action<EnemySpawner> OnBossEncounterCleared;

    private void Awake()
    {
        ManagerRegistry.TryGet(out _pool);
        SortWaveDatas();
    }

    private void Start()
    {
        if (_autoStart)
            StartFirstWave();
    }

    private void OnDestroy()
    {
        _isShuttingDown = true;
        ClearWaveRoutine();
        UnsubscribeBoss();
        UnsubscribeAllEnemies();
    }

    public void SetTarget(Transform target)
    {
        _target = target;

        if (_spawnedBoss != null)
            _spawnedBoss.SetTarget(_target);

        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            if (_spawnedEnemies[i] != null)
                _spawnedEnemies[i].SetTarget(_target);
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
                maxCount = waveData.enemyPrefabs.Count;
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

        SetBlocksActive(true);

        if (_isBossRoom)
        {
            if (_bossIntroRoutine != null)
            {
                StopCoroutine(_bossIntroRoutine);
                _bossIntroRoutine = null;
            }

            _bossIntroRoutine = StartCoroutine(CoStartBossEncounter());
            return;
        }

        if (_waveDatas == null || _waveDatas.Count == 0)
        {
            Debug.LogWarning($"{name}: WaveData가 없습니다.");
            return;
        }

        StartWave(0);
    }

    private IEnumerator CoStartBossEncounter()
    {
        SpawnBoss();

        if (_spawnedBoss == null)
        {
            _bossIntroRoutine = null;
            yield break;
        }

        _spawnedBoss.SetTarget(null);

        if (_bossIntroDelay > 0f)
            yield return new WaitForSeconds(_bossIntroDelay);

        if (_isShuttingDown || !this || _spawnedBoss == null || _spawnedBoss.IsDead)
        {
            _bossIntroRoutine = null;
            yield break;
        }

        _spawnedBoss.SetTarget(_target);

        if (_waveDatas != null && _waveDatas.Count > 0)
        {
            if (_reinforcementRoutine != null)
            {
                StopCoroutine(_reinforcementRoutine);
                _reinforcementRoutine = null;
            }

            _reinforcementRoutine = StartCoroutine(CoSpawnBossReinforcements());
        }

        _bossIntroRoutine = null;
    }

    private void SpawnBoss()
    {
        Transform spawnPoint = _bossSpawnPoint != null ? _bossSpawnPoint : transform;
        GameObject bossObject;

        if (_pool != null)
            bossObject = _pool.Get(_bossPrefab.gameObject, spawnPoint.position, spawnPoint.rotation);
        else
            bossObject = Instantiate(_bossPrefab.gameObject, spawnPoint.position, spawnPoint.rotation);

        DespawnController despawnController = bossObject.GetComponent<DespawnController>();
        if (despawnController != null)
            despawnController.SetMode(E_DespawnMode.ReturnToPool);

        _spawnedBoss = bossObject.GetComponent<EnemyBase>();
        if (_spawnedBoss == null)
        {
            Debug.LogWarning($"{name}: Boss Prefab에 EnemyBase가 없습니다.");
            return;
        }

        _spawnedBoss.OnDeathFinished -= HandleBossDeathFinished;
        _spawnedBoss.OnDeathFinished += HandleBossDeathFinished;
    }

    private IEnumerator CoSpawnBossReinforcements()
    {
        if (_reinforcementInitialDelay > 0f)
            yield return new WaitForSeconds(_reinforcementInitialDelay);

        while (!_isShuttingDown && this && gameObject.activeInHierarchy)
        {
            if (_spawnedBoss == null || _spawnedBoss.IsDead)
                yield break;

            SO_WaveData waveData = GetRandomReinforcementWave();
            if (waveData != null)
                yield return StartCoroutine(CoSpawnWave(waveData));

            if (_reinforcementInterval > 0f)
                yield return new WaitForSeconds(_reinforcementInterval);
            else
                yield return null;
        }

        _reinforcementRoutine = null;
    }

    private SO_WaveData GetRandomReinforcementWave()
    {
        if (_waveDatas == null || _waveDatas.Count == 0)
            return null;

        List<SO_WaveData> validWaveDatas = new();

        for (int i = 0; i < _waveDatas.Count; i++)
        {
            if (_waveDatas[i] != null)
                validWaveDatas.Add(_waveDatas[i]);
        }

        if (validWaveDatas.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, validWaveDatas.Count);
        return validWaveDatas[randomIndex];
    }

    private void SortWaveDatas()
    {
        _waveDatas.Sort((a, b) => a.id.CompareTo(b.id));
    }

    public void StartWave(int waveIndex)
    {
        if (_isBossRoom)
            return;

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

        if (_isBossRoom)
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
        if (_isBossRoom)
            return;

        if (_isShuttingDown || !this || !gameObject.activeInHierarchy)
            return;

        int nextIndex = _currentWaveIndex + 1;

        if (nextIndex >= _waveDatas.Count)
        {
            SetBlocksActive(false);
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

        if (!_isBossRoom)
        {
            _currentWaveKilledCount = 0;
            _isWaveSpawnCompleted = false;
            _isChangingWave = false;
        }

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
                    yield return new WaitForSeconds(waveData.spawnInterval);
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

        if (!_isBossRoom)
        {
            _isWaveSpawnCompleted = true;
            _spawnRoutine = null;
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab, Transform spawnPoint)
    {
        if (_isShuttingDown)
            return;

        if (enemyPrefab == null || spawnPoint == null)
            return;

        GameObject enemyObject;

        if (_pool != null)
            enemyObject = _pool.Get(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        else
            enemyObject = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        DespawnController despawnController = enemyObject.GetComponent<DespawnController>();
        if (despawnController != null)
            despawnController.SetMode(E_DespawnMode.ReturnToPool);

        EnemyBase enemyBase = enemyObject.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            enemyBase.SetTarget(_target);
            enemyBase.OnDeathFinished -= HandleEnemyDeathFinished;
            enemyBase.OnDeathFinished += HandleEnemyDeathFinished;

            if (!_spawnedEnemies.Contains(enemyBase))
                _spawnedEnemies.Add(enemyBase);
        }
    }

    private List<Transform> GetSpawnPointsForWave(int enemyCount)
    {
        List<Transform> shuffled = new List<Transform>(_spawnPoints);

        for (int i = 0; i < shuffled.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[randomIndex]) = (shuffled[randomIndex], shuffled[i]);
        }

        if (enemyCount <= shuffled.Count)
            return shuffled.GetRange(0, enemyCount);

        List<Transform> result = new List<Transform>(enemyCount);

        for (int i = 0; i < enemyCount; i++)
            result.Add(shuffled[i % shuffled.Count]);

        return result;
    }

    private void HandleBossDeathFinished(EnemyBase boss)
    {
        if (_isShuttingDown || !this)
            return;

        ClearWaveRoutine();

        if (_spawnedBoss != null)
        {
            _spawnedBoss.OnDeathFinished -= HandleBossDeathFinished;
            _spawnedBoss = null;
        }

        ClearRemainingEnemies();
        SetBlocksActive(false);
        OnBossEncounterCleared?.Invoke(this);
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

        if (_isBossRoom)
            return;

        if (_isChangingWave)
            return;

        _currentWaveKilledCount++;

        if (ShouldNextWave())
        {
            _isChangingWave = true;
            StartNextWave();
        }
    }

    private void ClearRemainingEnemies()
    {
        for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
        {
            EnemyBase enemy = _spawnedEnemies[i];
            if (enemy == null)
                continue;

            enemy.OnDeathFinished -= HandleEnemyDeathFinished;

            if (_pool != null)
                _pool.Return(enemy.gameObject);
            else
                Destroy(enemy.gameObject);
        }

        _spawnedEnemies.Clear();
    }

    private void UnsubscribeBoss()
    {
        if (_spawnedBoss != null)
        {
            _spawnedBoss.OnDeathFinished -= HandleBossDeathFinished;
            _spawnedBoss = null;
        }
    }

    private void UnsubscribeAllEnemies()
    {
        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            if (_spawnedEnemies[i] != null)
                _spawnedEnemies[i].OnDeathFinished -= HandleEnemyDeathFinished;
        }

        _spawnedEnemies.Clear();
    }

    private void SetBlocksActive(bool isActive)
    {
        if (_blocks == null)
            return;

        for (int i = 0; i < _blocks.Length; i++)
        {
            if (_blocks[i] != null)
                _blocks[i].SetActive(isActive);
        }
    }

    public void ClearWaveRoutine()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        if (_reinforcementRoutine != null)
        {
            StopCoroutine(_reinforcementRoutine);
            _reinforcementRoutine = null;
        }

        if (_bossIntroRoutine != null)
        {
            StopCoroutine(_bossIntroRoutine);
            _bossIntroRoutine = null;
        }
    }
}