using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private WeaponSO _weaponData;
    [SerializeField] private Transform _muzzle;
    [SerializeField] private PlayerProjectile _projectilePrefab;

    [SerializeField] private CameraShakeModule _cameraShake;

    private PlayerController _controller;
    private float _lastAttackTime = -100f;
    private int _currentAmmo;
    private bool _isReloading;
    private Coroutine _reloadRoutine;
    private PoolManager _poolManager;

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
        if (_poolManager != null && _projectilePrefab != null)
        {
            _poolManager.CreatePool(_projectilePrefab.gameObject, 30);
        }

        InitializeAmmo();
    }

    private void OnDisable()
    {
        if (_reloadRoutine != null)
        {
            StopCoroutine(_reloadRoutine);
            _reloadRoutine = null;
        }

        _isReloading = false;
    }

    public void InitializeAmmo()
    {
        if (_weaponData == null)
            return;

        _currentAmmo = _weaponData.MaxAmmo;
        _isReloading = false;

        OnAmmoChanged?.Invoke(_currentAmmo, _weaponData.MaxAmmo);
        OnReloadStateChanged?.Invoke(false);
    }

    public void HandleAttack()
    {
        if (_weaponData == null)
            return;

        if (_muzzle == null || _projectilePrefab == null)
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
            float rad = finalAngle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            GameObject projectileObject = _poolManager.Get(_projectilePrefab.gameObject, _muzzle.position, Quaternion.Euler(0f, 0f, finalAngle));

            PlayerProjectile projectile = projectileObject.GetComponent<PlayerProjectile>();
            projectile.Initialize(
                direction,
                _weaponData.ProjectileSpeed,
                _weaponData.ProjectileLifetime,
                _weaponData.Damage);
        }
    }
}