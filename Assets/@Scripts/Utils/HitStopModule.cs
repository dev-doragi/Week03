using System.Collections;
using UnityEngine;

public class HitStopModule : MonoBehaviour
{
    private static HitStopModule _instance;
    private float _originalFixedDeltaTime;
    private Coroutine _activeRoutine;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            _originalFixedDeltaTime = Time.fixedDeltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 게임 전체의 시간 흐름을 일시적으로 느리게 만듦
    /// </summary>
    /// <param name="duration">지속 시간(초)</param>
    /// <param name="timeScale">느려질 속도 비율 (0~1)</param>
    public static void Play(float duration, float timeScale = 0.05f)
    {
        if (_instance == null) return;

        if (_instance._activeRoutine != null)
            _instance.StopCoroutine(_instance._activeRoutine);

        _instance._activeRoutine = _instance.StartCoroutine(_instance.HitStopRoutine(duration, timeScale));
    }

    private IEnumerator HitStopRoutine(float duration, float targetScale)
    {
        Time.timeScale = targetScale;
        // 물리 연산 주기도 시간 배율에 맞춰 동기화
        Time.fixedDeltaTime = _originalFixedDeltaTime * targetScale;

        // 실제 시간 기준으로 대기 (Time.timeScale 무시)
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = _originalFixedDeltaTime;
        _activeRoutine = null;
    }

    private void OnDisable()
    {
        // 컴포넌트 비활성화 시 시간 정상화 안전장치
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _originalFixedDeltaTime;
    }
}