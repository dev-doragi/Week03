using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UI_Base : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] protected GameObject _firstSelectable;

    protected virtual void Awake()
    {
        BindEvents();
    }

    protected virtual void OnEnable()
    {
        // 패널 활성화 시 첫 버튼 선택 (패드 지원)
        //ApplyFirstSelection();
    }

    public void ApplyFirstSelection()
    {
        if (_firstSelectable != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_firstSelectable);
        }
    }

    // 각 패널에서 구현할 버튼 리스너 연결부
    protected abstract void BindEvents();

    //// 필요 시 리스너 해제용
    //protected abstract void UnbindEvents();

    //protected virtual void OnDestroy()
    //{
    //    UnbindEvents();
    //}
}