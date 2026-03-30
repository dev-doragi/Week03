using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float _interactionRange = 1.5f;
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private Transform _detectionOrigin;

    [Header("UI")]
    [SerializeField] private UI_ItemPrompt _promptUI;

    private PlayerInputReader _inputReader;
    private IInteractable _currentInteractable;

    private void Awake()
    {
        _inputReader = GetComponent<PlayerInputReader>();

        if (_detectionOrigin == null)
            _detectionOrigin = transform;
    }

    private void Update()
    {
        UpdateCurrentInteractable();
        HandleInteractInput();
    }

    private void UpdateCurrentInteractable()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            _detectionOrigin.position,
            _interactionRange,
            _interactableLayer);

        IInteractable nextInteractable = null;

        if (hit != null)
            nextInteractable = hit.GetComponent<IInteractable>();

        SetCurrentInteractable(nextInteractable);
    }

    private void SetCurrentInteractable(IInteractable nextInteractable)
    {
        if (_currentInteractable == nextInteractable)
            return;

        if (_currentInteractable != null)
            _currentInteractable.ShowOutline(false);

        _currentInteractable = nextInteractable;

        if (_currentInteractable != null)
            _currentInteractable.ShowOutline(true);

        if (_promptUI != null)
            _promptUI.SetTarget(_currentInteractable);
    }

    private void HandleInteractInput()
    {
        if (_currentInteractable == null)
            return;

        if (!_inputReader.WasInteractPressedThisFrame())
            return;

        _currentInteractable.Interact(gameObject);
    }

    private void OnDisable()
    {
        SetCurrentInteractable(null);
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = _detectionOrigin != null ? _detectionOrigin : transform;

        if (origin == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin.position, _interactionRange);
    }
}