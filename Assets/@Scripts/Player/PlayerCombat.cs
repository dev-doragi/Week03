using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Transform _weaponHoldPoint; // 무기 프리팹이 생성될 부모 트랜스폼
    [SerializeField] private CameraShakeModule _cameraShake;

    [Header("Starting Weapon")]
    [SerializeField] private WeaponSO _startingWeapon; // 인스펙터에서 할당할 기본 무기 SO

    private WeaponSO _weaponData;
    private Transform _muzzle; // 생성된 프리팹에서 추출할 총구 위치
    private GameObject _currentWeaponInstance;

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
        // 게임 시작 시 인스펙터에 할당된 SO가 있다면 장착 처리
        if (_startingWeapon != null)
        {
            EquipWeapon(_startingWeapon);
        }
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

    // 무기 장착 및 교체 로직
    public void EquipWeapon(WeaponSO newWeapon)
    {
        if (newWeapon == null) return;

        _weaponData = newWeapon;

        // 기존 장착된 무기 객체 제거
        if (_currentWeaponInstance != null)
        {
            Destroy(_currentWeaponInstance);
        }

        // 새 무기 프리팹 생성 및 초기화
        if (_weaponData.WeaponPrefab != null && _weaponHoldPoint != null)
        {
            _currentWeaponInstance = Instantiate(_weaponData.WeaponPrefab, _weaponHoldPoint, false);

            WeaponVisual visual = _currentWeaponInstance.GetComponent<WeaponVisual>();
            if (visual != null)
            {
                _muzzle = visual.MuzzleTransform;
            }
            else
            {
                Debug.LogWarning("Weapon 프리팹에 WeaponVisual 스크립트가 없습니다.");
                _muzzle = _currentWeaponInstance.transform; // Fallback
            }
        }

        // 새 무기의 투사체 풀링 초기화
        if (_poolManager != null && _weaponData.ProjectilePrefab != null)
        {
            _poolManager.CreatePool(_weaponData.ProjectilePrefab, 30);
        }

        InitializeAmmo();
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

        if (_muzzle == null || _weaponData.ProjectilePrefab == null)
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
            float rad = finalAngle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // 투사체를 WeaponSO에 정의된 프리팹으로 생성
            GameObject projectileObject = _poolManager.Get(_weaponData.ProjectilePrefab, _muzzle.position, Quaternion.Euler(0f, 0f, finalAngle));

            PlayerProjectile projectile = projectileObject.GetComponent<PlayerProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(
                    direction,
                    _weaponData.ProjectileSpeed,
                    _weaponData.ProjectileLifetime,
                    _weaponData.Damage);
            }
        }
    }
}