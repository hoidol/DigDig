using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class RewardPanel : MonoBehaviour
{
    public Image thumImage;
    public TMP_Text rewardValueText;

    // 값 갱신 강조: 글씨가 커지면서 새 값으로 바뀌고 원래 크기로 돌아옴
    public const float POP_SCALE = 1.6f;
    public const float POP_UP_TIME = 0.15f;
    public const float POP_DOWN_TIME = 0.3f;

    Sequence popSeq;

    public void SetRewardData(RewardData rewardData, UserReward userReward)
    {
        thumImage.sprite= rewardData.GetThum();
        rewardValueText.text =rewardData.GetValueToString();
    }

    // 2배 보상 등 값이 바뀔 때: 커진 순간 새 값으로 바꾸고 원래 크기로 (실시간, 일시정지 중에도 재생)
    public void PlayValueChange(RewardData rewardData)
    {
        Transform t = rewardValueText.transform;
        popSeq?.Kill();
        t.localScale = Vector3.one;

        popSeq = DOTween.Sequence()
            .Append(t.DOScale(POP_SCALE, POP_UP_TIME).SetEase(Ease.OutQuad))
            .AppendCallback(() => SetRewardData(rewardData, null))
            .Append(t.DOScale(1f, POP_DOWN_TIME).SetEase(Ease.OutBack))
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    void OnDisable()
    {
        popSeq?.Kill();
        if (rewardValueText != null)
            rewardValueText.transform.localScale = Vector3.one;
    }
}
