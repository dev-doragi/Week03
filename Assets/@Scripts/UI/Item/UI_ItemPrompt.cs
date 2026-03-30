using TMPro;
using UnityEngine;

public class UI_ItemPrompt : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private TMP_Text _promptText;
    [SerializeField] private Vector3 _worldOffset;

    private Transform _targetAnchor;
    private IInteractable _currentInteractable;

    public void SetTarget(IInteractable interactable)
    {
        _currentInteractable = interactable;
        _targetAnchor = interactable?.GetUIPivot();

        if (interactable != null)
        {
            _promptText.text = interactable.GetInteractionText();
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (_targetAnchor == null || _targetCamera == null || _rectTransform == null)
            return;

        Vector3 worldPosition = _targetAnchor.position + _worldOffset;
        Vector3 screenPosition = _targetCamera.WorldToScreenPoint(worldPosition);

        // 카메라 뒤에 있는 경우 숨김
        if (screenPosition.z <= 0f)
        {
            _rectTransform.gameObject.SetActive(false);
            return;
        }

        _rectTransform.gameObject.SetActive(true);
        _rectTransform.position = screenPosition;
    }
}