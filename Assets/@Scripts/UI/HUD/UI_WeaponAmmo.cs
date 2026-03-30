using TMPro;
using UnityEngine;

public class UI_WeaponAmmo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ammoText;
    [SerializeField] private GameObject _reloadText;

    private PlayerCombat _playerCombat;

    public void Bind(PlayerCombat playerCombat)
    {
        if (_playerCombat == playerCombat)
        {
            Refresh();
            return;
        }

        Unbind();
        _playerCombat = playerCombat;

        if (_playerCombat == null)
        {
            ResetView();
            return;
        }

        _playerCombat.OnAmmoChanged += HandleAmmoChanged;
        _playerCombat.OnReloadStateChanged += HandleReloadStateChanged;

        Refresh();
    }

    public void Unbind()
    {
        if (_playerCombat != null)
        {
            _playerCombat.OnAmmoChanged -= HandleAmmoChanged;
            _playerCombat.OnReloadStateChanged -= HandleReloadStateChanged;
            _playerCombat = null;
        }

        ResetView();
    }

    public void Refresh()
    {
        if (_playerCombat == null)
        {
            ResetView();
            return;
        }

        HandleAmmoChanged(_playerCombat.CurrentAmmo, _playerCombat.MaxAmmo);
        HandleReloadStateChanged(_playerCombat.IsReloading);
    }

    private void HandleAmmoChanged(int currentAmmo, int maxAmmo)
    {
        if (_ammoText != null)
            _ammoText.text = $"{currentAmmo} / {maxAmmo}";
    }

    private void HandleReloadStateChanged(bool isReloading)
    {
        if (_reloadText != null)
            _reloadText.SetActive(isReloading);
    }

    private void ResetView()
    {
        if (_ammoText != null)
            _ammoText.text = string.Empty;

        if (_reloadText != null)
            _reloadText.SetActive(false);
    }
}