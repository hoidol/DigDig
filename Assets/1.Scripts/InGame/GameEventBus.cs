using System;
using System.Collections.Generic;

public static class GameEventBus
{
    static readonly Dictionary<Type, List<Delegate>> handlers
        = new Dictionary<Type, List<Delegate>>();

    // 구독
    public static void Subscribe<T>(Action<T> handler)
    {
        Type type = typeof(T);
        if (!handlers.ContainsKey(type))
            handlers[type] = new List<Delegate>();

        handlers[type].Add(handler);
    }

    // 구독 해제
    public static void Unsubscribe<T>(Action<T> handler)
    {
        Type type = typeof(T);
        if (handlers.ContainsKey(type))
            handlers[type].Remove(handler);
    }

    // 발행
    public static void Publish<T>(T evt)
    {
        Type type = typeof(T);
        if (!handlers.ContainsKey(type)) return;

        foreach (Delegate handler in handlers[type])
            (handler as Action<T>)?.Invoke(evt);
    }

    // 씬 전환 시 전체 초기화
    public static void Clear()
    {
        handlers.Clear();
    }

}