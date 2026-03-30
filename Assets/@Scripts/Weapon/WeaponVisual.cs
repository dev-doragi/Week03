using UnityEngine;

// 무기 프리팹의 최상단 오브젝트에 붙이는 스크립트
public class WeaponVisual : MonoBehaviour
{
    [Tooltip("이 무기의 총구(발사 위치) 트랜스폼을 연결하세요.")]
    public Transform MuzzleTransform;
}