using System;
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

    // 광고 호출은 이걸로. AdManager가 아직 없으면(실제 광고 연결 전, StartSetting에서 생성 안 함) 광고 없이 성공 처리
    public static void Show(string ad_id, AdType adType, Action<bool> callback)
    {
        if (SkipAdForTest)
        {
            Debug.Log($"[Ad] 테스트: 광고 생략 → 성공 ({ad_id})");
            callback?.Invoke(true);
            return;
        }
        if (Instance == null)
        {
            Debug.LogWarning($"[Ad] AdManager 없음 → 광고 없이 성공 처리 ({ad_id})");
            callback?.Invoke(true);
            return;
        }
        Instance.Play(ad_id, adType, callback);
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
