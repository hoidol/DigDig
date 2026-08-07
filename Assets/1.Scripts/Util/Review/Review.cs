#if UNITY_ANDROID
using Google.Play.Review;
#endif
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
public class Review : MonoSingleton<Review>
{
#if UNITY_ANDROID
    private ReviewManager reviewManager;
#endif


    // ...
    public void Request()
    {
#if UNITY_ANDROID
        reviewManager = new ReviewManager();
        StartCoroutine(CoRequest());
#elif UNITY_IOS
        Device.RequestStoreReview();
#else
#endif


    }
#if UNITY_ANDROID
    IEnumerator CoRequest()
    {
        var requestFlowOperation = reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            yield break;
        }
        PlayReviewInfo playReviewInfo = requestFlowOperation.GetResult();
        var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
        yield return launchFlowOperation;

        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.LogError("Failed to launch review flow: " + launchFlowOperation.Error);
            yield break;
        }

        Debug.Log("Review flow completed successfully.");
        Destroy(gameObject);

    }
#endif
}
