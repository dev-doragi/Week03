using UnityEngine;

public class TutorialEnemyUIAnchor : MonoBehaviour
{
    [SerializeField] private Transform _uiAnchor;

    public Transform UIAnchor => _uiAnchor != null ? _uiAnchor : transform;
}