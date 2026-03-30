using UnityEngine;

[CreateAssetMenu(fileName = "SO_ItemData", menuName = "Scriptable Objects/Item/WeaponItem")]
public class SO_ItemData : ScriptableObject
{
    [field: SerializeField] public WeaponSO WeaponData { get; private set; } 
    [field: SerializeField] public Sprite NormalSprite { get; private set; } 
    [field: SerializeField] public Sprite OutlineSprite { get; private set; } 
    [field: SerializeField] public string InteractionMessage { get; private set; } = "줍기";
}
