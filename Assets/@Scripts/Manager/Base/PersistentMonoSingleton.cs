using UnityEngine;

// 싱글톤 패턴을 구현한 클래스
// 모든 Core 매니저 클래스는 PersistentMonoSingleton을 상속받아야 한다.
// Core 매니저만 사용한다.

public abstract class PersistentMonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
        DontDestroyOnLoad(gameObject);

        OnInitialized();
    }

    /// <summary>
    /// 싱글톤 초기화 이후 호출
    /// </summary>
    protected virtual void OnInitialized() { }
}