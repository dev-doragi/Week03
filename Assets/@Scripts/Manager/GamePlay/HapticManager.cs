using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HapticManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    private Coroutine _hapticRoutine;
    private int _currentPriority = -1;
    private float _currentStrength = -1f;
    private float _endTime;

    public void Initialize()
    {
        if (IsInitialized)
            return;

        IsInitialized = true;
    }

    public void PlayOneShot(float lowFrequency, float highFrequency, float duration, int priority = 0)
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad == null) return;

        Debug.Log("Haptic Excuted.");

        float newStrength = Mathf.Max(lowFrequency, highFrequency);
        bool isPlaying = _hapticRoutine != null && Time.unscaledTime < _endTime;

        if (isPlaying)
        {
            Debug.Log($"{ _currentPriority}");

            if (priority < _currentPriority)
                return;

            if (priority == _currentPriority && newStrength <= _currentStrength)
                return;
        }

        if (_hapticRoutine != null)
            StopCoroutine(_hapticRoutine);

        _currentPriority = priority;
        _currentStrength = newStrength;
        _endTime = Time.unscaledTime + duration;

        _hapticRoutine = StartCoroutine(CoPlayOneShot(gamepad, lowFrequency, highFrequency, duration));
    }

    public void PlayPlayerHit()
    {
        PlayOneShot(0.6f, 1f, 0.15f, 10);
    }

    public void StopHaptics()
    {
        if (_hapticRoutine != null)
        {
            StopCoroutine(_hapticRoutine);
            _hapticRoutine = null;
        }

        _currentPriority = -1;
        _currentStrength = -1f;
        _endTime = 0f;

        if (Gamepad.current != null)
            Gamepad.current.ResetHaptics();
    }

    private IEnumerator CoPlayOneShot(Gamepad gamepad, float lowFrequency, float highFrequency, float duration)
    {
        gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
        yield return new WaitForSecondsRealtime(duration);

        if (gamepad != null)
            gamepad.ResetHaptics();

        _hapticRoutine = null;
        _currentPriority = -1;
        _currentStrength = -1f;
        _endTime = 0f;
    }

    private void OnDisable()
    {
        StopHaptics();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            StopHaptics();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            StopHaptics();
    }
}