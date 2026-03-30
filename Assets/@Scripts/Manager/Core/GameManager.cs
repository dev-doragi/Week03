using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PersistentMonoSingleton<GameManager>
{
    [Header("Core Managers")]
    [SerializeField] private GameStateManager _gameStateManager;
    [SerializeField] private PoolManager _poolManager;
    [SerializeField] private PauseController _pauseController;
    [SerializeField] private HapticManager _hapticManager;
    [SerializeField] private UIManager _uiManager;

    [Header("Dungeon")]
    [SerializeField] private DungeonGenerator _dungeonGenerator;
    [SerializeField] private DungeonSpawnerBuilder _dungeonSpawnerBuilder;

    [SerializeField] private bool _autoStartInEditor = true;

    [Header("Death Sequence")]
    [SerializeField] private float _deathSlowTimeScale = 0.5f;
    [SerializeField] private float _deathSequenceDuration = 0.75f;

    [Header("Clear Sequence")]
    [SerializeField] private float _clearSlowTimeScale = 0.5f;
    [SerializeField] private float _clearSequenceDuration = 0.75f;

    private PlayerController _playerController;
    private PlayerHealth _playerHealth;
    private PlayerCombat _playerCombat;
    private Coroutine _deathRoutine;
    private Coroutine _clearRoutine;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        RegisterManagers();
        InitializeManagers();

        SceneManager.sceneLoaded += OnSceneLoaded;

        SubscribeDungeonEvents();
        BindPlayer();
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (_autoStartInEditor)
        {
        }
#else
        StartGame();
#endif
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isPlayableScene = scene.name == "StageScene" || scene.name == "TutorialScene";
        if (!isPlayableScene)
            return;

        RebindStageReferences();
        _uiManager?.RebindSceneUI();
        StartCoroutine(CoStartStageNextFrame());
    }

    private IEnumerator CoStartStageNextFrame()
    {
        yield return null;
        StartGame();
    }

    private void RebindStageReferences()
    {
        UnsubscribeDungeonEvents();

        _dungeonGenerator = FindAnyObjectByType<DungeonGenerator>();
        _dungeonSpawnerBuilder = FindAnyObjectByType<DungeonSpawnerBuilder>();

        SubscribeDungeonEvents();
    }

    private void SubscribeDungeonEvents()
    {
        if (_dungeonGenerator != null)
            _dungeonGenerator.OnDungeonGenerated += HandleDungeonGenerated;

        if (_dungeonSpawnerBuilder != null)
            _dungeonSpawnerBuilder.OnBossRoomCleared += HandleBossRoomCleared;
    }

    private void UnsubscribeDungeonEvents()
    {
        if (_dungeonGenerator != null)
            _dungeonGenerator.OnDungeonGenerated -= HandleDungeonGenerated;

        if (_dungeonSpawnerBuilder != null)
            _dungeonSpawnerBuilder.OnBossRoomCleared -= HandleBossRoomCleared;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeDungeonEvents();

        if (_deathRoutine != null)
        {
            StopCoroutine(_deathRoutine);
            _deathRoutine = null;
        }

        if (_clearRoutine != null)
        {
            StopCoroutine(_clearRoutine);
            _clearRoutine = null;
        }

        UnbindPlayer();
    }

    private void RegisterManagers()
    {
        if (_gameStateManager == null)
        {
            Debug.LogError("GameStateManager is not assigned!");
            return;
        }

        if (_poolManager == null)
        {
            Debug.LogError("PoolManager is not assigned!");
            return;
        }

        if (_pauseController == null)
        {
            Debug.LogError("PauseController is not assigned!");
            return;
        }

        if (_hapticManager == null)
        {
            Debug.LogError("HapticManager is not assigned!");
            return;
        }

        if (_uiManager == null)
        {
            Debug.LogError("UIManager is not assigned!");
            return;
        }

        ManagerRegistry.Register<GameManager>(this);
        ManagerRegistry.Register<GameStateManager>(_gameStateManager);
        ManagerRegistry.Register<PoolManager>(_poolManager);
        ManagerRegistry.Register<PauseController>(_pauseController);
        ManagerRegistry.Register<HapticManager>(_hapticManager);
        ManagerRegistry.Register<UIManager>(_uiManager);
    }

    private void InitializeManagers()
    {
        Initialize(_gameStateManager);
        Initialize(_poolManager);
        Initialize(_pauseController);
        Initialize(_hapticManager);
        Initialize(_uiManager);
    }

    private void Initialize(IInitializable manager)
    {
        if (manager == null)
            return;

        if (!manager.IsInitialized)
            manager.Initialize();
    }

    private void BindPlayer()
    {
        UnbindPlayer();

        _playerController = FindAnyObjectByType<PlayerController>();
        if (_playerController == null)
            return;

        _playerHealth = _playerController.GetComponent<PlayerHealth>();
        if (_playerHealth == null)
            return;

        _playerCombat = _playerController.GetComponent<PlayerCombat>();
        if (_playerCombat == null)
            return;

        if (_dungeonSpawnerBuilder != null)
            _dungeonSpawnerBuilder.SetSpawnTarget(_playerController.transform);

        _playerHealth.OnDeathStarted += HandleDeath;
    }

    private void UnbindPlayer()
    {
        if (_playerHealth != null)
            _playerHealth.OnDeathStarted -= HandleDeath;

        _playerHealth = null;
        _playerCombat = null;
        _playerController = null;
    }

    public void StartGame()
    {
        BeginRunSetup();
    }

    public void BeginRunSetup()
    {
        if (_dungeonGenerator == null)
        {
            Debug.LogError("DungeonGenerator is missing!");
            return;
        }

        if (_deathRoutine != null)
        {
            StopCoroutine(_deathRoutine);
            _deathRoutine = null;
        }

        if (_clearRoutine != null)
        {
            StopCoroutine(_clearRoutine);
            _clearRoutine = null;
        }

        Time.timeScale = 1f;
        _poolManager?.ClearRuntimeObjects();

        if (_playerController != null)
            _playerController.SetControlEnabled(false);

        _gameStateManager.ChangeState(GameState.Loading);
        _dungeonGenerator.GenerateDungeon();
    }

    private void HandleDungeonGenerated(DungeonLayout layout)
    {
        BindPlayer();

        if (_playerController == null || _playerHealth == null || _playerCombat == null)
            return;

        Vector3 spawnPosition = GetPlayerSpawnPosition();

        _playerController.transform.position = spawnPosition;
        _playerController.transform.rotation = Quaternion.identity;

        Rigidbody2D rigidbody2D = _playerController.Rigidbody;
        if (rigidbody2D != null)
        {
            rigidbody2D.linearVelocity = Vector2.zero;
            rigidbody2D.angularVelocity = 0f;
        }

        _playerHealth.ResetHp();
        _playerController.ResetRuntimeState();
        _playerCombat.ResetForNewRun();

        PlayerAnimController animController = _playerController.GetComponent<PlayerAnimController>();
        if (animController != null)
            animController.ResetAnimationState();

        _playerController.gameObject.SetActive(true);

        _uiManager?.RebindSceneUI();
        _uiManager?.RebindUI(_playerHealth, _playerCombat);

        SetPlayerControlEnabled(true);
        _gameStateManager.ChangeState(GameState.Playing);
    }

    private Vector3 GetPlayerSpawnPosition()
    {
        if (_dungeonGenerator == null)
            return Vector3.zero;

        Vector2Int spawnTile = _dungeonGenerator.PlayerSpawnTile;
        return new Vector3(spawnTile.x + 0.5f, spawnTile.y + 0.5f, 0f);
    }

    private void HandleDeath()
    {
        if (_deathRoutine != null)
            StopCoroutine(_deathRoutine);

        _deathRoutine = StartCoroutine(CoHandleDeath());
    }

    private IEnumerator CoHandleDeath()
    {
        SetPlayerControlEnabled(false);
        _gameStateManager.ChangeState(GameState.Death);
        Time.timeScale = _deathSlowTimeScale;

        yield return new WaitForSecondsRealtime(_deathSequenceDuration);

        _gameStateManager.ChangeState(GameState.GameOver);
        Time.timeScale = 0f;

        _deathRoutine = null;
    }

    private void HandleBossRoomCleared()
    {
        if (_clearRoutine != null)
            return;

        if (_deathRoutine != null)
            return;

        _clearRoutine = StartCoroutine(CoHandleClear());
    }

    private IEnumerator CoHandleClear()
    {
        SetPlayerControlEnabled(false);
        _gameStateManager.ChangeState(GameState.Clear);
        Time.timeScale = _clearSlowTimeScale;

        yield return new WaitForSecondsRealtime(_clearSequenceDuration);

        Time.timeScale = 0f;
        _clearRoutine = null;
    }

    public void SetPlayerControlEnabled(bool enabled)
    {
        if (_playerController == null)
            return;

        _playerController.SetControlEnabled(enabled);
    }

    public void DebugRestartGame()
    {
        if (_playerController == null)
        {
            Debug.LogWarning("PlayerController not found.");
            return;
        }

        if (_playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth not found.");
            return;
        }

        Time.timeScale = 1f;

        Rigidbody2D rigidbody2D = _playerController.Rigidbody;
        if (rigidbody2D != null)
        {
            rigidbody2D.linearVelocity = Vector2.zero;
            rigidbody2D.angularVelocity = 0f;
        }

        _playerController.ResetRuntimeState();
        _playerHealth.ResetHp();
        _playerCombat.ResetForNewRun();
        _playerController.gameObject.SetActive(true);

        SetPlayerControlEnabled(true);
        _gameStateManager.ChangeState(GameState.Playing);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        BeginRunSetup();
    }
}