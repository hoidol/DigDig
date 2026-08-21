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
