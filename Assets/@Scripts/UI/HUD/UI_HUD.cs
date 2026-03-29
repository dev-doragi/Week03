using UnityEngine;

public class UI_HUD : MonoBehaviour
{
    [SerializeField] private UI_HpBar _hpBar;
    [SerializeField] private UI_WeaponAmmo _weaponAmmo;

    public void Bind(PlayerHealth playerHealth, PlayerCombat playerCombat)
    {
        if (_hpBar != null)
            _hpBar.Bind(playerHealth);

        if (_weaponAmmo != null)
            _weaponAmmo.Bind(playerCombat);
    }

    public void Unbind()
    {
        if (_hpBar != null)
            _hpBar.Unbind();

        if (_weaponAmmo != null)
            _weaponAmmo.Unbind();
    }
}