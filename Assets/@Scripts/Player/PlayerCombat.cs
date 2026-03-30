using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    private const float INITIAL_LAST_ATTACK_TIME = -100f;
    private const int PROJECTILE_POOL_SIZE = 30;

    [SerializeField] private Transform _weaponHoldPoint;
    [SerializeField] private CameraShakeModule _cameraShake;

    [Header("Starting Weapon")]
    [SerializeField] private WeaponSO _startingWeapon;

    private WeaponSO _weaponData;
    private Transform _muzzle;
    private GameObject _currentWeaponInstance;

    private PlayerController _controller;
    private PoolManager _poolManager;

    private float _lastAttackTime = INITIAL_LAST_ATTACK_TIME;
    private int _currentAmmo;
    private bool _isReloading;
    private Coroutine _reloadRoutine;

    public WeaponSO WeaponData => _weaponData;
    public int CurrentAmmo => _currentAmmo;
    public int MaxAmmo => _weaponData != null ? _weaponData.MaxAmmo : 0;
    public bool IsReloading => _isReloading;

    public event Action<int, int> OnAmmoChanged;
    public event Action<bool> OnReloadStateChanged;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        ManagerRegistry.TryGet(out _poolManager);
    }

    private void Start()
    {
        if (_startingWeapon != null)
            EquipWeapon(_startingWeapon);
        else
            InitializeAmmo();
    }

    private void OnDisable()
    {
        StopReloadRoutine();
    }

    public void EquipWeapon(WeaponSO newWeapon)
    {
        if (newWeapon == null)
            return;

        StopReloadRoutine();

        _weaponData = newWeapon;
        _lastAttackTime = INITIAL_LAST_ATTACK_TIME;

        RebuildWeaponInstance();
        EnsureProjectilePool();
        InitializeAmmo();
    }

    public void ResetForNewRun()
    {
        StopReloadRoutine();
        _lastAttackTime = INITIAL_LAST_ATTACK_TIME;

        if (_startingWeapon != null)
        {
            EquipWeapon(_startingWeapon);
            return;
        }

        ClearCurrentWeaponInstance();
        _weaponData = null;
        _currentAmmo = 0;

        OnAmmoChanged?.Invoke(0, 0);
        OnReloadStateChanged?.Invoke(false);
    }

    public void InitializeAmmo()
    {
        StopReloadRoutine();
        _lastAttackTime = INITIAL_LAST_ATTACK_TIME;

        if (_weaponData == null)
        {
            _currentAmmo = 0;
            OnAmmoChanged?.Invoke(0, 0);
            OnReloadStateChanged?.Invoke(false);
            return;
        }

        _currentAmmo = _weaponData.MaxAmmo;
        OnAmmoChanged?.Invoke(_currentAmmo, _weaponData.MaxAmmo);
        OnReloadStateChanged?.Invoke(false);
    }

    public void HandleAttack()
    {
        if (_weaponData == null)
            return;

        if (_muzzle == null)
            return;

        if (_weaponData.ProjectilePrefab == null)
            return;

        if (_poolManager == null)
            return;

        if (_controller.IsDashing)
            return;

        if (_isReloading)
            return;

        if (!_controller.IsAttackPressed)
            return;

        if (Time.time < _lastAttackTime + _weaponData.AttackInterval)
            return;

        if (_currentAmmo <= 0)
        {
            TryStartReload();
            return;
        }

        if (_cameraShake != null)
            _cameraShake.Play(0.15f);

        Fire();

        _currentAmmo--;
        _lastAttackTime = Time.time;

        OnAmmoChanged?.Invoke(_currentAmmo, _weaponData.MaxAmmo);
        _controller.RaiseAttackPerformed();

        if (_currentAmmo <= 0)
            TryStartReload();
    }

    public void HandleReload()
    {
        if (!_controller.IsReloadPressedThisFrame)
            return;

        TryStartReload();
    }

    private void TryStartReload()
    {
        if (_weaponData == null)
            return;

        if (_isReloading)
            return;

        if (_currentAmmo >= _weaponData.MaxAmmo)
            return;

        if (_reloadRoutine != null)
            return;

        _reloadRoutine = StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        OnReloadStateChanged?.Invoke(true);

        yield return new WaitForSeconds(_weaponData.ReloadDuration);

        _currentAmmo = _weaponData.MaxAmmo;
        _isReloading = false;
        _reloadRoutine = null;

        OnAmmoChanged?.Invoke(_currentAmmo, _weaponData.MaxAmmo);
        OnReloadStateChanged?.Invoke(false);
    }

    private void Fire()
    {
        int pelletCount = Mathf.Max(1, _weaponData.PelletCount);
        float baseAngle = _muzzle.eulerAngles.z;

        for (int i = 0; i < pelletCount; i++)
        {
            float angleOffset = pelletCount == 1
                ? 0f
                : UnityEngine.Random.Range(-_weaponData.SpreadAngle, _weaponData.SpreadAngle);

            float finalAngle = baseAngle + angleOffset;
            float radian = finalAngle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));

            GameObject projectileObject = _poolManager.Get(
                _weaponData.ProjectilePrefab,
                _muzzle.position,
                Quaternion.Euler(0f, 0f, finalAngle));

            if (projectileObject == null)
                continue;

            PlayerProjectile projectile = projectileObject.GetComponent<PlayerProjectile>();
            if (projectile == null)
                continue;

            projectile.Initialize(
                direction,
                _weaponData.ProjectileSpeed,
                _weaponData.ProjectileLifetime,
                _weaponData.Damage);
        }
    }

    private void RebuildWeaponInstance()
    {
        ClearCurrentWeaponInstance();

        if (_weaponData == null)
        {
            _muzzle = GetDefaultMuzzle();
            return;
        }

        if (_weaponData.WeaponPrefab == null || _weaponHoldPoint == null)
        {
            _muzzle = GetDefaultMuzzle();
            return;
        }

        _currentWeaponInstance = Instantiate(_weaponData.WeaponPrefab, _weaponHoldPoint, false);

        WeaponVisual visual = _currentWeaponInstance.GetComponent<WeaponVisual>();
        if (visual != null && visual.MuzzleTransform != null)
            _muzzle = visual.MuzzleTransform;
        else
            _muzzle = _currentWeaponInstance.transform;
    }

    private void ClearCurrentWeaponInstance()
    {
        if (_currentWeaponInstance != null)
            Destroy(_currentWeaponInstance);

        _currentWeaponInstance = null;
        _muzzle = null;
    }

    private void EnsureProjectilePool()
    {
        if (_poolManager == null)
            return;

        if (_weaponData == null)
            return;

        if (_weaponData.ProjectilePrefab == null)
            return;

        _poolManager.CreatePool(_weaponData.ProjectilePrefab, PROJECTILE_POOL_SIZE);
    }

    private void StopReloadRoutine()
    {
        if (_reloadRoutine != null)
        {
            StopCoroutine(_reloadRoutine);
            _reloadRoutine = null;
        }

        if (_isReloading)
        {
            _isReloading = false;
            OnReloadStateChanged?.Invoke(false);
        }
    }

    private Transform GetDefaultMuzzle()
    {
        if (_weaponHoldPoint != null)
            return _weaponHoldPoint;

        return transform;
    }
}