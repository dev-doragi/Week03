using UnityEngine;

public class UI_HUD : MonoBehaviour
{
    [SerializeField] private UI_HpBar _hpBar;
    [SerializeField] private UI_WeaponAmmo _weaponAmmo;

    public void Bind(Player player)
    {
        if (player == null)
            return;

        if (_hpBar != null)
            _hpBar.Bind(player.playerHealth);

        if (_weaponAmmo != null)
            _weaponAmmo.Bind(player.playerCombat);
    }

    public void Unbind()
    {
        if (_hpBar != null)
            _hpBar.Unbind();

        if (_weaponAmmo != null)
            _weaponAmmo.Unbind();
    }
}