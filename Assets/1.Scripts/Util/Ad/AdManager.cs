using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AdManager : MonoSingleton<AdManager>
{
    [SerializeField] private AdNetwork adNetwork;

    void Awake()
    {
        if (adNetwork == null)
        {
            adNetwork = GetComponentInChildren<AdNetwork>();
        }
    }

    // 테스트용: 켜면 광고를 띄우지 않고 바로 성공 (플레이 테스트가 켜고 끝나면 끔. 기본 꺼짐)
    public static bool SkipAdForTest;

    // 광고 호출은 이걸로 (호출하는 쪽에서 광고 제거 여부를 따로 확인하지 않는다)
    // - 광고 제거 구매자: 광고 없이 성공
    // - AdManager가 아직 없으면(실제 광고 연결 전, StartSetting에서 생성 안 함): 광고 없이 성공
    // 성공하면 광고 시청 업적(Watch_Ad) +1 — 광고 없이 성공한 경우도 포함
    public static void Show(string ad_id, AdType adType, Action<bool> callback)
    {
        if (UserDataManager.Instance != null && UserDataManager.Instance.isAdRemoved)
        {
            Succeed(callback);
            return;
        }
        if (SkipAdForTest)
        {
            Debug.Log($"[Ad] 테스트: 광고 생략 → 성공 ({ad_id})");
            Succeed(callback);
            return;
        }
        if (Instance == null)
        {
            Debug.LogWarning($"[Ad] AdManager 없음 → 광고 없이 성공 처리 ({ad_id})");
            Succeed(callback);
            return;
        }
        Instance.Play(ad_id, adType, async (success) =>
        {
            await UniTask.SwitchToMainThread(); // 광고 SDK 콜백은 메인 스레드가 아닐 수 있음
            if (success)
                Succeed(callback);
            else
                callback?.Invoke(false);
        });
    }

    static void Succeed(Action<bool> callback)
    {
        AchievementManager.Report(AchievementType.Watch_Ad);
        callback?.Invoke(true);
    }

    // ad_id를 비워두면 Admob에 설정된 기본(테스트) 광고 단위 ID가 사용된다
    public void Play(string ad_id, AdType adType, Action<bool> callback)
    {
        if (adNetwork == null)
        {
            Debug.LogError("[Ad] AdNetwork 컴포넌트를 찾을 수 없습니다.");
            callback?.Invoke(false);
            return;
        }

        switch (adType)
        {
            case AdType.IS:
                adNetwork.ShowInterstitial(ad_id, callback);
                break;
            case AdType.RA:
                adNetwork.ShowRewarded(ad_id, callback);
                break;
            case AdType.BN:
                adNetwork.ShowBanner(ad_id, callback);
                break;
        }
    }
}
public enum AdType
{
    IS,
    RA,
    BN
}
