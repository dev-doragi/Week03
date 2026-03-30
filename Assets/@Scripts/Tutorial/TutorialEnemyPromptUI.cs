using TMPro;
using UnityEngine;

public class TutorialEnemyPromptUI : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private Transform _targetAnchor;
    [SerializeField] private TMP_Text _promptText;
    [SerializeField] private Vector3 _worldOffset;

    public void SetTarget(Transform targetAnchor)
    {
        _targetAnchor = targetAnchor;
    }

    public void SetText(string message)
    {
        if (_promptText != null)
            _promptText.text = message;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_targetAnchor == null || _targetCamera == null || _rectTransform == null)
            return;

        Vector3 worldPosition = _targetAnchor.position + _worldOffset;
        Vector3 screenPosition = _targetCamera.WorldToScreenPoint(worldPosition);

        if (screenPosition.z <= 0f)
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
            return;
        }

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        _rectTransform.position = screenPosition;
    }
}