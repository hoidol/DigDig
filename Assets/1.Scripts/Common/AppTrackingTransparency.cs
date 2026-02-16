using UnityEngine;
using System;
using System.Collections;
#if UNITY_IOS
using UnityEngine.iOS;
using Unity.Advertisement.IosSupport;
#endif
public class AppTrackingTransparency : MonoBehaviour
{
    public event Action<int> onTrackingAuthorizationComplete;

    public void StartAppTracking(Action<int> callback)
    {
        onTrackingAuthorizationComplete = callback;
#if UNITY_IOS
        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
            ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
            StartCoroutine(WaitForTrackingResponse());
        }
        else
        {
            onTrackingAuthorizationComplete?.Invoke((int)ATTrackingStatusBinding.GetAuthorizationTrackingStatus());
        }
#endif
    }
#if UNITY_IOS
    public IEnumerator WaitForTrackingResponse()
    {
        // 트래킹 상태가 변경될 때까지 기다림
        while (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
               ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            yield return new WaitForSeconds(0.5f);
        }

        int status = (int)ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

        string idfa = Device.advertisingIdentifier;
        Debug.Log($"IDFA: {idfa}");

        onTrackingAuthorizationComplete?.Invoke(status);
        Debug.Log($"Tracking Authorization Status: {status}");
    }
#endif
}
