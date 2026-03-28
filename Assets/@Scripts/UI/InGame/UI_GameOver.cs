using UnityEngine;
using UnityEngine.UI;

public class UI_GameOver : UI_Base
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;

    // 외부(UIManager 등)에서 구독할 이벤트
    public event System.Action OnRetryRequested;
    public event System.Action OnMainMenuRequested;

    protected override void Awake()
    {
        base.Awake(); // BindEvents 실행
    }

    protected override void BindEvents()
    {
        // 버튼 리스너 연결
        _restartButton.onClick.AddListener(() => {
            OnRetryRequested?.Invoke();
            Debug.Log("Retry Clicked");
        });

        _mainMenuButton.onClick.AddListener(() => {
            OnMainMenuRequested?.Invoke();
            Debug.Log("Main Menu Clicked");
        });
    }
}