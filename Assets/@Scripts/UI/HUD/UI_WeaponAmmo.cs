using TMPro;
using UnityEngine;

public class UI_WeaponAmmo : MonoBehaviour
{
    [SerializeField] private PlayerCombat _playerCombat;
    [SerializeField] private TextMeshProUGUI _ammoText;
    [SerializeField] private GameObject _reloadText;

    private void Start()
    {
        Bind(_playerCombat);
    }

    private void OnDestroy()
    {
        Unbind();
    }

    public void Bind(PlayerCombat playerCombat)
    {
        Unbind();

        _playerCombat = playerCombat;

        if (_playerCombat == null)
            return;

        _playerCombat.OnAmmoChanged += HandleAmmoChanged;
        _playerCombat.OnReloadStateChanged += HandleReloadStateChanged;

        Refresh();
    }

    public void Unbind()
    {
        if (_playerCombat == null)
            return;

        _playerCombat.OnAmmoChanged -= HandleAmmoChanged;
        _playerCombat.OnReloadStateChanged -= HandleReloadStateChanged;
        _playerCombat = null;
    }

    private void HandleAmmoChanged(int currentAmmo, int maxAmmo)
    {
        if (_ammoText == null)
            return;

        _ammoText.text = $"{currentAmmo} / {maxAmmo}";
    }

    private void HandleReloadStateChanged(bool isReloading)
    {
        if (_reloadText != null)
            _reloadText.SetActive(isReloading);
    }

    private void Refresh()
    {
        if (_playerCombat == null)
            return;

        HandleAmmoChanged(_playerCombat.CurrentAmmo, _playerCombat.MaxAmmo);
        HandleReloadStateChanged(_playerCombat.IsReloading);
    }
}