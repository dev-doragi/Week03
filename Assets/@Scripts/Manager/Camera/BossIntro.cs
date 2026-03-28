using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class BossIntro : MonoBehaviour
{
    public static event Action OnEndIntro;
    public static event Action OnPlayerDisable;

    [SerializeField] private float _endTime = 4f;
    [SerializeField] private CinemachineCamera _main;
    [SerializeField] private CinemachineCamera _intro;
    [SerializeField] private GameObject speedEffect;

    

    private void OnEnable()
    {
        CameraManager.OnBossIntro += RunIntro;
    }

    private void OnDisable()
    {
        CameraManager.OnBossIntro -= RunIntro;
    }

    private void Start()
    {
        
    }

    public void RunIntro()
    {
        StartCoroutine(EndIntro());
    }

    IEnumerator EndIntro()
    {
        OnPlayerDisable?.Invoke();
        yield return new WaitForSeconds(_endTime);
        if(speedEffect != null )
            speedEffect.SetActive(false);
        _main.Priority = 10;
        _intro.Priority = 5;
        yield return new WaitForSeconds(2);
        OnEndIntro?.Invoke();

    }
}
