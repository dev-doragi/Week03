using System;
using System.Collections.Generic;

// 인터페이스 기반 접근을 위한 매니저 레지스트리
// 매니저를 등록하고 조회할 수 있는 간단한 서비스 컨테이너 역할을 한다.

public static class ManagerRegistry
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Register<T>(T instance) where T : class
    {
        _services[typeof(T)] = instance;
    }

    public static T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
            return (T)service;

        throw new InvalidOperationException($"Service not registered: {typeof(T).Name}");
    }

    public static bool TryGet<T>(out T instance) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            instance = (T)service;
            return true;
        }

        instance = null;
        return false;
    }

    public static void Clear()
    {
        _services.Clear();
    }
}