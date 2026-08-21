using System;
using UnityEngine;

// 광고 네트워크 공통 기반 클래스 (현재는 AdMob 구현체만 존재)
public abstract class AdNetwork : MonoBehaviour
{
    public bool IsInitialized { get; protected set; }

    public abstract void ShowInterstitial(string adUnitId, Action<bool> callback);
    public abstract void ShowRewarded(string adUnitId, Action<bool> callback);
    public abstract void ShowBanner(string adUnitId, Action<bool> callback);
}
