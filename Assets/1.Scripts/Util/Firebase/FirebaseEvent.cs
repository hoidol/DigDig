using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
#if !UNITY_EDITOR
using Firebase.Analytics;
#endif

// Firebase Analytics 이벤트 기록. 어디서든 한 줄로 호출:
//   FirebaseEvent.Log(FirebaseEventName.STAGE_START);
//   FirebaseEvent.Log(FirebaseEventName.STAGE_END, ("difficulty", "Normal"), ("wave", 12));
// - Firebase 초기화 전에 호출해도 큐에 쌓았다가 초기화되면 보냄 (초기화 실패 시 버림)
// - 에디터에서는 Firebase 네이티브 라이브러리가 없으므로 콘솔 로그만 찍음
// - 이름 규칙: 영문·숫자·_ / 영문으로 시작 / 40자 이하, 파라미터 25개 이하, 문자열 값 100자 이하
//   (firebase_, google_, ga_ 로 시작하는 이름은 예약어라 금지)
public static class FirebaseEvent
{
    const int MAX_QUEUE = 100;
    static readonly Regex NAME_RULE = new Regex("^[A-Za-z][A-Za-z0-9_]{0,39}$");

    struct Pending
    {
        public string name;
        public (string key, object value)[] parameters;
    }

    static readonly Queue<Pending> queue = new Queue<Pending>();
    static bool subscribed;

    public static void Log(string eventName, params (string key, object value)[] parameters)
    {
        if (!IsValidName(eventName))
        {
            Debug.LogWarning($"[FirebaseEvent] 잘못된 이벤트 이름: {eventName}");
            return;
        }

#if UNITY_EDITOR
        Debug.Log($"[FirebaseEvent] {eventName} {Describe(parameters)}");
#else
        FirebaseManager mgr = FirebaseManager.Instance;
        if (mgr != null && mgr.IsInitialized)
        {
            Send(eventName, parameters);
            return;
        }
        if (mgr != null && mgr.IsInitFailed)
            return;

        if (queue.Count >= MAX_QUEUE) queue.Dequeue();
        queue.Enqueue(new Pending { name = eventName, parameters = parameters });
        if (!subscribed)
        {
            subscribed = true;
            FirebaseManager.OnInitialized += Flush;
        }
#endif
    }

    // 로그인 후 uid 등 (Firebase 콘솔에서 유저별로 이벤트를 볼 때)
    public static void SetUserId(string userId)
    {
#if !UNITY_EDITOR
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsInitialized)
            FirebaseAnalytics.SetUserId(userId);
#endif
    }

    // 유저 속성 (예: 최고 난이도, 유저 레벨). 이름 24자 이하, 값 36자 이하
    public static void SetUserProperty(string name, string value)
    {
#if !UNITY_EDITOR
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsInitialized)
            FirebaseAnalytics.SetUserProperty(name, value);
#else
        Debug.Log($"[FirebaseEvent] user property {name} = {value}");
#endif
    }

    public static bool IsValidName(string name)
    {
        if (string.IsNullOrEmpty(name) || !NAME_RULE.IsMatch(name)) return false;
        return !(name.StartsWith("firebase_") || name.StartsWith("google_") || name.StartsWith("ga_"));
    }

    static string Describe((string key, object value)[] parameters)
    {
        if (parameters == null || parameters.Length == 0) return "";
        var parts = new string[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
            parts[i] = $"{parameters[i].key}={parameters[i].value}";
        return "{ " + string.Join(", ", parts) + " }";
    }

#if !UNITY_EDITOR
    static void Flush(bool success)
    {
        FirebaseManager.OnInitialized -= Flush;
        subscribed = false;
        while (queue.Count > 0)
        {
            Pending p = queue.Dequeue();
            if (success) Send(p.name, p.parameters);
        }
    }

    static void Send(string eventName, (string key, object value)[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
        {
            FirebaseAnalytics.LogEvent(eventName);
            return;
        }

        var list = new List<Parameter>(parameters.Length);
        foreach (var (key, value) in parameters)
        {
            if (!IsValidName(key))
            {
                Debug.LogWarning($"[FirebaseEvent] {eventName}: 잘못된 파라미터 이름 {key}");
                continue;
            }
            switch (value)
            {
                case int v: list.Add(new Parameter(key, v)); break;
                case long v: list.Add(new Parameter(key, v)); break;
                case float v: list.Add(new Parameter(key, v)); break;
                case double v: list.Add(new Parameter(key, v)); break;
                case bool v: list.Add(new Parameter(key, v ? 1L : 0L)); break;
                default:
                    string s = value?.ToString() ?? "";
                    list.Add(new Parameter(key, s.Length > 100 ? s.Substring(0, 100) : s));
                    break;
            }
        }
        FirebaseAnalytics.LogEvent(eventName, list.ToArray());
    }
#endif
}
