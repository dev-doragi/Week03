using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UI_Base : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] private GameObject _firstSelectable;
    [SerializeField] private bool _applyFirstSelectionOnEnable = true;

    private bool _isBound;

    protected virtual void Awake()
    {
        CacheReferences();
    }

    protected virtual void OnEnable()
    {
        BindBase();

        if (_applyFirstSelectionOnEnable)
            ApplyFirstSelection();

        RefreshUI();
    }

    protected virtual void OnDisable()
    {
        UnbindBase();
    }

    protected virtual void OnDestroy()
    {
        UnbindBase();
    }

    public void ApplyFirstSelection()
    {
        if (_firstSelectable == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_firstSelectable);
    }

    protected void BindBase()
    {
        if (_isBound)
            return;

        BindUI();
        _isBound = true;
    }

    protected void UnbindBase()
    {
        if (!_isBound)
            return;

        UnbindUI();
        _isBound = false;
    }

    protected void RebindBase()
    {
        UnbindBase();
        BindBase();
    }

    protected virtual void CacheReferences() { }
    protected virtual void RefreshUI() { }

    protected abstract void BindUI();
    protected abstract void UnbindUI();
}