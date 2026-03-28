//using Unity.VectorGraphics.Editor;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }


    [SerializeField] private BGMPlayer _bgmPlayer;
    [SerializeField] private SFXPlayer _sfxPlayer;
    [SerializeField] private BGM[] _bgmEntry;
    [SerializeField] private SFX[] _sfxEntry;
    [SerializeField] private float _bgmVolume = 1f;
    [SerializeField] private float _sfxVolume = 1f;
    [SerializeField] private float _minPitch = 0.5f;
    [SerializeField] private float _pitchLerpSpeed = 10f;

    private float _targetPitch = 1f;
    private float _currentPitch = 1f;

    void OnEnable()
    {
        //CameraManager.OnBossOutro += HandleBossStart;

    }

    void OnDisable()
    {
        //CameraManager.OnBossOutro -= HandleBossStart;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        PlayBGM(0);
    }
    private void Update()
    {
        if (Mathf.Approximately(_currentPitch, _targetPitch))
            return;

        _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, Time.unscaledDeltaTime * _pitchLerpSpeed);

        if (Mathf.Abs(_currentPitch - _targetPitch) < 0.01f)
            _currentPitch = _targetPitch;

        ApplyGlobalPitch(_currentPitch);
    }

    private void PlayBGM(int number, bool fade = true)
    {
        if (number == 1)
            _bgmVolume = .34f;
        else
            _bgmVolume = .2f;
        AudioClip clip = _bgmEntry[number]._clip;
        _bgmPlayer.Play(clip, fade, _bgmVolume);
    }
    public void StopBGM()
    {
        _bgmPlayer.Stop();
        Debug.Log("사운드");
    }
    private void PlaySFX(int number)
    {
        AudioClip clip = _sfxEntry[number]._clip;
        _sfxPlayer.Play(clip, _sfxVolume);
    }

    public void HandleMainStart()
    {
        PlayBGM(0);
    }
    public void HandleBossStart()
    {
        PlayBGM(1);
    }
    public void HandlePistolSFX()
    {
        PlaySFX(0);
    }
    public void HandleShotGunSFX()
    {
        PlaySFX(1);
    }

    public void SetSlowAudio(float timeScale)
    {
        _targetPitch = Mathf.Clamp(timeScale, _minPitch, 1f);
    }

    public void ResetSlowAudio()
    {
        _targetPitch = 1f;
    }

    private void ApplyGlobalPitch(float pitch)
    {
        if (_bgmPlayer != null)
            _bgmPlayer.SetPitch(pitch);

        if (_sfxPlayer != null)
            _sfxPlayer.SetPitch(pitch);
    }

    private void ApplyGlobalPitchImmediate(float pitch)
    {
        _currentPitch = pitch;
        _targetPitch = pitch;
        ApplyGlobalPitch(pitch);
    }
}
