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

    private PlayerController _playerController;
    private PlayerHealth _playerHealth;
    private PlayerCombat _playerCombat;
    private Coroutine _deathRoutine;

    #region Debugging
    [ContextMenu("Debug Die")]
    private void DebugDie()
    {
        if (_playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth not found.");
            return;
        }

        _playerHealth.TakeDamage(9999);
    }

    [ContextMenu("Debug Restart")]
    private void DebugRestart()
    {
#if UNITY_EDITOR
        DebugRestartGame();
#else
        RestartGame();
#endif
    }
    #endregion

    protected override void OnInitialized()
    {
        base.OnInitialized();

        RegisterManagers();
        InitializeManagers();

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (_dungeonGenerator != null)
        {
            _dungeonGenerator.OnDungeonGenerated += HandleDungeonGenerated;
        }
        else
        {
            Debug.LogWarning("DungeonGenerator is not assigned.");
        }

        if (_dungeonSpawnerBuilder == null)
        {
            _dungeonSpawnerBuilder = FindAnyObjectByType<DungeonSpawnerBuilder>();
        }

        BindPlayer();

        Debug.Log("GameManager Initialized");
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (_autoStartInEditor)
        {
            StartGame();
        }
#else
        StartGame();
#endif
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindPlayer();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (_dungeonGenerator != null)
        {
            _dungeonGenerator.OnDungeonGenerated -= HandleDungeonGenerated;
        }

        if (_deathRoutine != null)
        {
            StopCoroutine(_deathRoutine);
            _deathRoutine = null;
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
        {
            _dungeonSpawnerBuilder.SetSpawnTarget(_playerController.transform);
        }

        _playerHealth.OnDeathStarted += HandleDeath;
    }

    private void UnbindPlayer()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnDeathStarted -= HandleDeath;
        }

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

        Debug.Log("Run Setup Begin");

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

        if (_playerController == null)
        {
            Debug.LogError("PlayerController not found after dungeon generation.");
            return;
        }

        if (_playerHealth == null)
        {
            Debug.LogError("PlayerHealth not found after dungeon generation.");
            return;
        }

        if (_uiManager != null)
        {
            _uiManager.RebindUI();
        }

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
        _playerCombat.InitializeAmmo();

        PlayerAnimController animController = _playerController.GetComponent<PlayerAnimController>();
        if (animController != null)
        {
            animController.ResetAnimationState();
        }

        _playerController.gameObject.SetActive(true);

        SetPlayerControlEnabled(true);
        _gameStateManager.ChangeState(GameState.Playing);

        Debug.Log("Dungeon generated and player initialized.");
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
        {
            StopCoroutine(_deathRoutine);
        }

        _deathRoutine = StartCoroutine(CoHandleDeath());
    }

    private IEnumerator CoHandleDeath()
    {
        Debug.Log("Player Die");

        SetPlayerControlEnabled(false);
        _gameStateManager.ChangeState(GameState.Death);
        Time.timeScale = _deathSlowTimeScale;

        yield return new WaitForSecondsRealtime(_deathSequenceDuration);

        _gameStateManager.ChangeState(GameState.GameOver);
        Time.timeScale = 0f;

        _deathRoutine = null;
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
        _playerCombat.InitializeAmmo();
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