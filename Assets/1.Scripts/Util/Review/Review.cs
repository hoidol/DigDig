#if UNITY_ANDROID
using Google.Play.Review;
#endif
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Review
{
#if UNITY_ANDROID
    private ReviewManager reviewManager;
#endif


    // Request() 3번 호출마다 1번씩만 실제 리뷰 요청
    public void Request()
    {

#if UNITY_ANDROID
        RequestAndroidReview().Forget();
#elif UNITY_IOS
        Device.RequestStoreReview();
#endif
    }

#if UNITY_ANDROID
    async UniTaskVoid RequestAndroidReview()
    {
        reviewManager = new ReviewManager();
        var requestFlowOperation = reviewManager.RequestReviewFlow();
        await UniTask.WaitUntil(() => requestFlowOperation.IsDone);
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            return;

        PlayReviewInfo playReviewInfo = requestFlowOperation.GetResult();
        var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
        await UniTask.WaitUntil(() => launchFlowOperation.IsDone);

        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.LogError("Failed to launch review flow: " + launchFlowOperation.Error);
            return;
        }

        Debug.Log("Review flow completed successfully.");
    }
#endif
}
