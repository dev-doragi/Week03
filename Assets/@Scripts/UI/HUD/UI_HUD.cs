using UnityEngine;

public class UI_HUD : UI_Base
{
    [SerializeField] private UI_HpBar _hpBar;
    [SerializeField] private UI_WeaponAmmo _weaponAmmo;

    private PlayerHealth _playerHealth;
    private PlayerCombat _playerCombat;

    public void Bind(PlayerHealth playerHealth, PlayerCombat playerCombat)
    {
        _playerHealth = playerHealth;
        _playerCombat = playerCombat;

        RebindBase();
        RefreshUI();
    }

    public void Unbind()
    {
        UnbindBase();

        _playerHealth = null;
        _playerCombat = null;

        if (_hpBar != null)
            _hpBar.Unbind();

        if (_weaponAmmo != null)
            _weaponAmmo.Unbind();
    }

    protected override void BindUI()
    {
        if (_hpBar != null)
            _hpBar.Bind(_playerHealth);

        if (_weaponAmmo != null)
            _weaponAmmo.Bind(_playerCombat);
    }

    protected override void UnbindUI()
    {
        if (_hpBar != null)
            _hpBar.Unbind();

        if (_weaponAmmo != null)
            _weaponAmmo.Unbind();
    }

    protected override void RefreshUI()
    {
        if (_hpBar != null)
            _hpBar.Refresh();

        if (_weaponAmmo != null)
            _weaponAmmo.Refresh();
    }
}