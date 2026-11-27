using System;
using UnityEngine;

// (2026-11-27) 서버 시각 (ms, UTC 기준 Unix 시간): 기기 시각 + Firebase .info/serverTimeOffset (공용 DB)
// 플레이 기록의 브레이크타임 시작 시각에 씀. 아직 못 받았거나 에디터(Firebase 없음)면 기기 시각 (IsSynced = false)
public static class ServerClock
{
    static long offsetMs;
    static bool listening;

    public static bool IsSynced { get; private set; }

    public static long NowMs
    {
        get
        {
            EnsureListening();
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + offsetMs;
        }
    }

    static void EnsureListening()
    {
        if (listening || Application.isEditor || FirebaseManager.Instance == null || !FirebaseManager.Instance.IsInitialized)
            return;
        listening = true;
        try
        {
            FirebaseDatabaseConfig.Common.GetReference(".info/serverTimeOffset").ValueChanged += (s, e) =>
            {
                if (e.Snapshot?.Value == null)
                    return;
                offsetMs = Convert.ToInt64(e.Snapshot.Value);
                IsSynced = true;
            };
        }
        catch (Exception e)
        {
            listening = false;
            Debug.LogWarning($"[ServerClock] 서버 시각 구독 실패 : {e.Message}");
        }
    }
}
