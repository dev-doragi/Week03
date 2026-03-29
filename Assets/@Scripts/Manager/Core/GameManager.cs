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

    private PlayerController _playerController;
    private PlayerHealth _playerHealth;

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
        RestartGame();
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

        if (_dungeonSpawnerBuilder != null && _playerController != null)
        {
            _dungeonSpawnerBuilder.SetSpawnTarget(_playerController.transform);
        }

        _playerHealth.OnDie += HandlePlayerDie;
    }

    private void UnbindPlayer()
    {
        if (_playerHealth == null)
            return;

        _playerHealth.OnDie -= HandlePlayerDie;
        _playerHealth = null;
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

        Debug.Log("Run Setup Begin");

        Time.timeScale = 1f;
        SetPlayerControlEnabled(false);
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
            Debug.Log("Rebinding UI");
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

        _playerController.ResetRuntimeState();
        _playerHealth.ResetHp();
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

    private void HandlePlayerDie()
    {
        Debug.Log("Player Die");

        SetPlayerControlEnabled(false);

        if (_playerController != null)
        {
            _playerController.gameObject.SetActive(false);
            Debug.Log("Player Disabled");
        }

        _gameStateManager.ChangeState(GameState.GameOver);
        Debug.Log("GameState -> GameOver");

        Time.timeScale = 0f;
        Debug.Log("Time scaled set 0");
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        if (_playerController == null)
            return;

        _playerController.SetControlEnabled(enabled);
    }

    public void RestartGame()
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

        Rigidbody2D rigidbody2D = _playerController.Rigidbody;
        if (rigidbody2D != null)
        {
            rigidbody2D.linearVelocity = Vector2.zero;
            rigidbody2D.angularVelocity = 0f;
        }

        _playerController.ResetRuntimeState();
        _playerHealth.ResetHp();

        _playerController.gameObject.SetActive(true);

        SetPlayerControlEnabled(true);
        _gameStateManager.ChangeState(GameState.Playing);
    }
}