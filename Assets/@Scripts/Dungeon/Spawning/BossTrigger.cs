using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossTrigger : MonoBehaviour
{
    [SerializeField] private BossSpawner _bossSpawner;
    [SerializeField] private bool _triggerOnce = true;

    private bool _isTriggered;

    private void Reset()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggerOnce && _isTriggered)
            return;

        if (other.GetComponent<PlayerController>() == null)
            return;

        if (_bossSpawner == null)
            return;

        _isTriggered = true;
        _bossSpawner.ActivateEncounter();
    }
}