using UnityEngine;

public class DropItem : MonoBehaviour, IInteractable
{
    private SO_ItemData _itemData;

    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Transform _uiPivot;

    // 상자에서 생성 시 호출
    public void Setup(SO_ItemData data)
    {
        _itemData = data;

        if (_itemData != null && _renderer != null)
        {
            _renderer.sprite = _itemData.NormalSprite;
        }
    }

    public void ShowOutline(bool show)
    {
        if (_renderer == null || _itemData == null) return;
        _renderer.sprite = show ? _itemData.OutlineSprite : _itemData.NormalSprite;
    }

    public Transform GetUIPivot() => _uiPivot != null ? _uiPivot : transform;

    public void Interact(GameObject interactor)
    {
        if (_itemData == null || _itemData.WeaponData == null) return;

        PlayerCombat combat = interactor.GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.EquipWeapon(_itemData.WeaponData); //
            Destroy(gameObject);
        }
    }

    public string GetInteractionText()
    {
        return _itemData != null ? $"{_itemData.WeaponData.name} {_itemData.InteractionMessage}" : "아이템 줍기";
    }
}