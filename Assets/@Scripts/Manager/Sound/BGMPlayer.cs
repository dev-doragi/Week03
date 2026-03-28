using System.Collections;
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{

    [SerializeField] private AudioSource sourceA;
    [SerializeField] private AudioSource sourceB;
    [SerializeField] private float fadeDuration = 1f;

    private AudioSource _currentSource;
    private AudioSource _nextSource;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        _currentSource = sourceA;
        _nextSource = sourceB;
    }

    public void Play(AudioClip clip, bool fade, float volume)
    {
        Debug.Log(clip);
        if (_currentSource.clip == clip && _currentSource.isPlaying)
            return;

        if (fade)
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(CoCrossFade(clip, volume));
        }
        else
        {
            _currentSource.Stop();
            _currentSource.clip = clip;
            _currentSource.volume = volume;
            _currentSource.loop = true;
            _currentSource.Play();
        }
    }

    public void Stop()
    {
        _currentSource.Stop();
        _nextSource.Stop();
    }

    private IEnumerator CoCrossFade(AudioClip clip, float targetVolume)
    {
        _nextSource.clip = clip;
        Debug.Log(clip);
        Debug.Log(_nextSource.clip);

        _nextSource.loop = true;
        _nextSource.volume = 0f;
        _nextSource.Play();

        float elapsed = 0f;
        float startVolume = _currentSource.volume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            _currentSource.volume = Mathf.Lerp(startVolume, 0f, t);
            _nextSource.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        _currentSource.Stop();

        var temp = _currentSource;
        _currentSource = _nextSource;
        _nextSource = temp;

        _currentSource.volume = targetVolume;
        _nextSource.volume = 0f;
    }
    public void SetPitch(float pitch)
    {
        _currentSource.pitch = pitch;
        _nextSource.pitch = pitch;
    }
}
