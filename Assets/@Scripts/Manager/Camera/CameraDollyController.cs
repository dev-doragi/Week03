using System.Collections;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class CameraDollyController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _dollyCam;
    [SerializeField] private CinemachineCamera _mainCam;

    [SerializeField] private CinemachineSplineDolly _splineDolly;
    [SerializeField] private CameraCutScene _cutScene;
    [SerializeField] private SplineContainer splineContainer;

    [SerializeField] private float _moveDuration = 2f;


    public void PlayerBossIntro(GameObject mainCam, GameObject dollyCam, CameraCutScene cutScene)
    {
        _dollyCam = dollyCam.GetComponent<CinemachineCamera>();
        _mainCam = mainCam.GetComponent<CinemachineCamera>();
        _cutScene = cutScene;
        StartCoroutine(BossCutScene());
    }

    IEnumerator BossCutScene()
    {

        var spline = splineContainer.Spline;

        spline.SetKnot(0, new BezierKnot(new float3(_mainCam.gameObject.transform.position.x, _mainCam.gameObject.transform.position.y, _mainCam.gameObject.transform.position.z)));
        spline.SetKnot(1, new BezierKnot(new float3(_cutScene.BossPos.x, _cutScene.BossPos.y, _cutScene.BossPos.z)));
        
        _splineDolly.CameraPosition = 0f;
        _dollyCam.Priority = 10;
        _mainCam.Priority = 5;
        float elapsed = 0f;
        while (elapsed < _moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _moveDuration);

            _splineDolly.CameraPosition = Mathf.Lerp(0, 1, t);

            yield return null;
        }
        _splineDolly.CameraPosition = 1;
        yield return new WaitForSeconds(3f);
        Debug.Log(_cutScene.BossPos);
        _mainCam.transform.position = _cutScene.BossPos;
        _mainCam.Priority = 15;
        _mainCam.Lens.OrthographicSize = _cutScene.BossCamSize;
        _dollyCam.Priority = 5;
    }
}
