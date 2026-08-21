using System;
using GoogleMobileAds.Api;
using UnityEngine;

// AdMob SDK 초기화 및 전면/보상형/배너 광고 로드-노출을 담당하는 구현체
public class Admob : AdNetwork
{
    // TODO: 실제 출시 전 AdMob App ID를 실제 값으로 교체할 것
    // App ID는 코드가 아니라 Assets > Google Mobile Ads > Settings 에서 설정한다
    // Android App ID : ca-app-pub-xxxxxxxxxxxxxxxx~xxxxxxxxxx
    // iOS App ID     : ca-app-pub-xxxxxxxxxxxxxxxx~xxxxxxxxxx

    // 아래 광고 단위 ID는 전부 Google 공식 테스트 ID이며, ad_id를 넘기지 않았을 때만 사용되는 기본값이다
    // TODO: 실제 출시 전 각 광고 단위 ID를 실제 값으로 교체할 것
#if UNITY_ANDROID
    private const string DefaultInterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    private const string DefaultRewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
    private const string DefaultBannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IOS
    private const string DefaultInterstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";
    private const string DefaultRewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313";
    private const string DefaultBannerAdUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
    private const string DefaultInterstitialAdUnitId = "unused";
    private const string DefaultRewardedAdUnitId = "unused";
    private const string DefaultBannerAdUnitId = "unused";
#endif

    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private BannerView bannerView;

    void Start()
    {
        MobileAds.Initialize(_ =>
        {
            IsInitialized = true;
            Debug.Log("[Ad] AdMob 초기화 완료");
        });
    }

    public override void ShowInterstitial(string adUnitId, Action<bool> callback)
    {
        if (!IsInitialized)
        {
            Debug.LogError("[Ad] AdMob이 아직 초기화되지 않았습니다.");
            callback?.Invoke(false);
            return;
        }

        string unitId = string.IsNullOrEmpty(adUnitId) ? DefaultInterstitialAdUnitId : adUnitId;

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        InterstitialAd.Load(unitId, new AdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError($"[Ad] 전면 광고 로드 실패 : {error}");
                callback?.Invoke(false);
                return;
            }

            interstitialAd = ad;
            interstitialAd.OnAdFullScreenContentClosed += () => callback?.Invoke(true);
            interstitialAd.OnAdFullScreenContentFailed += fullScreenError =>
            {
                Debug.LogError($"[Ad] 전면 광고 노출 실패 : {fullScreenError}");
                callback?.Invoke(false);
            };

            interstitialAd.Show();
        });
    }

    public override void ShowRewarded(string adUnitId, Action<bool> callback)
    {
        if (!IsInitialized)
        {
            Debug.LogError("[Ad] AdMob이 아직 초기화되지 않았습니다.");
            callback?.Invoke(false);
            return;
        }

        string unitId = string.IsNullOrEmpty(adUnitId) ? DefaultRewardedAdUnitId : adUnitId;

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        RewardedAd.Load(unitId, new AdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError($"[Ad] 보상형 광고 로드 실패 : {error}");
                callback?.Invoke(false);
                return;
            }

            rewardedAd = ad;
            bool rewardEarned = false;

            rewardedAd.OnAdFullScreenContentClosed += () => callback?.Invoke(rewardEarned);
            rewardedAd.OnAdFullScreenContentFailed += fullScreenError =>
            {
                Debug.LogError($"[Ad] 보상형 광고 노출 실패 : {fullScreenError}");
                callback?.Invoke(false);
            };

            rewardedAd.Show(_ => rewardEarned = true);
        });
    }

    public override void ShowBanner(string adUnitId, Action<bool> callback)
    {
        if (!IsInitialized)
        {
            Debug.LogError("[Ad] AdMob이 아직 초기화되지 않았습니다.");
            callback?.Invoke(false);
            return;
        }

        string unitId = string.IsNullOrEmpty(adUnitId) ? DefaultBannerAdUnitId : adUnitId;

        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }

        bannerView = new BannerView(unitId, AdSize.Banner, AdPosition.Bottom);
        bannerView.OnBannerAdLoaded += () => callback?.Invoke(true);
        bannerView.OnBannerAdLoadFailed += error =>
        {
            Debug.LogError($"[Ad] 배너 광고 로드 실패 : {error}");
            callback?.Invoke(false);
        };

        bannerView.LoadAd(new AdRequest());
    }

    void OnDestroy()
    {
        interstitialAd?.Destroy();
        rewardedAd?.Destroy();
        bannerView?.Destroy();
    }
}
