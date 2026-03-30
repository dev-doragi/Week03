using UnityEngine;

public class TutorialUIBinder : MonoBehaviour
{
    [SerializeField] private TutorialEnemyUIAnchor _targetEnemy;
    [SerializeField] private TutorialEnemyPromptUI _enemyPromptUI;
    [SerializeField] private string _message = "공격해보세요";

    private void Start()
    {
        if (_targetEnemy == null || _enemyPromptUI == null)
            return;

        _enemyPromptUI.SetTarget(_targetEnemy.UIAnchor);
        _enemyPromptUI.SetText(_message);
        _enemyPromptUI.Show();
    }
}