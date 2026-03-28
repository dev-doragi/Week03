using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static event Action OnBossIntro;
    public static event Action OnBossOutro;
    public static CameraManager Instance { get; private set; }

    [SerializeField] private BossIntro bossIntro;

    public enum CameraMode
    {
        Follow,
        ZoneFollow,
        Locked,
        Boss,
    }

    [SerializeField] private CameraBoundsController _boundsController;
    [SerializeField] private CameraZoneController _zoneController;
    [SerializeField] private CameraDollyController _dollyController;

    [SerializeField] private Transform _playerZoneCameraAnchor;
    [SerializeField] private Transform _playerDefaultCameraAnchor;
    [SerializeField] private float _baseCameraSize=15f;
    public GameObject _cineCam;
    public GameObject _cutSceneCam;


    public CameraZone CurrentZone { get; private set; }
    public CameraMode CurrentMode = CameraMode.Follow;

    private void OnEnable()
    {
        BossIntro.OnEndIntro += BossOutro;
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if(bossIntro != null)
        {
            StartBossIntro();
        }
        CurrentMode = CameraMode.Follow;
        SetZone(null);
    }

    public void SetZone(CameraZone zone)
    {
        CurrentZone = zone;
        if(zone != null)
            CurrentMode = CameraMode.ZoneFollow;
        else
            CurrentMode = CameraMode.Follow;
        _zoneController.ApplyZone(zone);
        SettingFollow();
    }

    public void ClearZone(CameraZone zone)
    {
        if (CurrentZone != zone) return;

        CurrentZone = null;
        CurrentMode = CameraMode.Follow;
        _zoneController.ApplyZone(null);
        SettingFollow();
    }

    public void SettingFollow()
    {
        if(CurrentMode != CameraMode.Follow)
            _cineCam.GetComponent<CinemachineCamera>().Target.TrackingTarget = _playerZoneCameraAnchor;
        else
            _cineCam.GetComponent<CinemachineCamera>().Target.TrackingTarget = _playerDefaultCameraAnchor;
    }

    public void SetSize(float size)
    {
        _cineCam.GetComponent<CinemachineCamera>().Lens.OrthographicSize = size;
    }

    public void ResetSize()
    {
        _cineCam.GetComponent<CinemachineCamera>().Lens.OrthographicSize = _baseCameraSize;
    }

    public void BossIntroCutScene(CameraCutScene cutScene)
    {
        _dollyController.PlayerBossIntro(_cineCam, _cutSceneCam, cutScene);
    }

    public void StartBossIntro()
    {
        OnBossIntro?.Invoke();
        OnBossIntro = null;
    }

    public void BossOutro()
    {
        SoundManager.instance.HandleBossStart();
        OnBossOutro?.Invoke();
        OnBossOutro = null;
    }
}
