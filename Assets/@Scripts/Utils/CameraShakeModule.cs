using UnityEngine;

public class CameraShakeModule : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] private Transform _target;
    [SerializeField] private float _frequency = 25f;    // 진동 속도
    [SerializeField] private float _baseAmplitude = 0.2f; // 기본 진폭

    public static CameraShakeModule Instance { get; private set; }

    private float _timer;
    private float _shakeDuration;
    private float _currentAmplitude;
    private float _noiseTime;
    private Vector3 _initialLocalPos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (_target == null)
            _target = transform;

        _initialLocalPos = _target.localPosition;
    }

    private void LateUpdate()
    {
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;

            // 남은 시간 비율에 따른 선형 감쇠 (1 -> 0)
            float t = Mathf.Clamp01(_timer / _shakeDuration);
            _noiseTime += Time.deltaTime * _frequency;

            // Perlin Noise 기반 좌표 산출 (연속적인 무작위성)
            float x = (Mathf.PerlinNoise(_noiseTime, 0f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0f, _noiseTime) - 0.5f) * 2f;

            _target.localPosition = _initialLocalPos + new Vector3(x, y, 0) * (_currentAmplitude * t);
        }
        else
        {
            // 진동 종료 시 초기 위치로 복구
            _target.localPosition = _initialLocalPos;
        }
    }

    /// <summary>
    /// 지정된 시간과 강도로 흔들림 발생
    /// </summary>
    public void Play(float duration, float strengthMultiplier = 1f)
    {
        _shakeDuration = duration;
        _timer = duration;
        _currentAmplitude = _baseAmplitude * strengthMultiplier;
    }
}