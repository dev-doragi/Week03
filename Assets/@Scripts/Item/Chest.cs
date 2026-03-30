using UnityEngine;
using DG.Tweening;

public class InteractionChest : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _outlineSprite;
    [SerializeField] private Transform _uiPivot;

    [Header("Standard Drop Settings")]
    [SerializeField] private SO_ItemData[] _dropTable;
    [SerializeField] private GameObject _pickupPrefab;

    [Header("Special Drop Settings")]
    [SerializeField] private SO_ItemData _specialItem;
    [SerializeField, Range(0f, 1f)] private float _specialDropChance = 0.1f;

    private bool _isOpened;
    private Vector3 _defaultScale;

    private void Awake()
    {
        _defaultScale = transform.localScale;
    }

    public void ShowOutline(bool show)
    {
        if (_renderer != null)
            _renderer.sprite = show ? _outlineSprite : _normalSprite;
    }

    public void Interact(GameObject interactor)
    {
        if (_isOpened)
            return;

        _isOpened = true;

        transform.DOKill();

        Sequence sequence = DOTween.Sequence();

        sequence.Append(transform.DOScale(
            new Vector3(_defaultScale.x * 1.08f, _defaultScale.y * 0.92f, _defaultScale.z),
            0.08f));

        sequence.Append(transform.DOScale(
            new Vector3(_defaultScale.x * 0.92f, _defaultScale.y * 1.08f, _defaultScale.z),
            0.1f));

        sequence.Append(transform.DOScale(_defaultScale, 0.06f));

        sequence.AppendCallback(SpawnItem);

        sequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void SpawnItem()
    {
        SO_ItemData selectedItem = null;

        if (_specialItem != null && Random.value <= _specialDropChance)
        {
            selectedItem = _specialItem;
        }
        else if (_dropTable != null && _dropTable.Length > 0)
        {
            selectedItem = _dropTable[Random.Range(0, _dropTable.Length)];
        }

        if (selectedItem == null)
            return;

        GameObject go = Instantiate(
            _pickupPrefab,
            transform.position + Vector3.up * 0.5f,
            Quaternion.identity);

        DropItem dropItem = go.GetComponent<DropItem>();

        if (dropItem != null)
        {
            dropItem.Setup(selectedItem);
        }
    }

    public string GetInteractionText() => "열기";

    public Transform GetUIPivot()
    {
        return _uiPivot != null ? _uiPivot : transform;
    }
}