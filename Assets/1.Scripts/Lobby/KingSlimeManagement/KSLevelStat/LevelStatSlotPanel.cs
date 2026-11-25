using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-24) 용사 강화 한 칸 (스탯 또는 능력, KingLevelStatSetting.StatOption) (한 줄 = 한 묶음, 이전 권능 칸 AuthoritySlotPanel을 바꿈. 씬 연결 유지 위해 guid 그대로)
// 누르면 1포인트 찍음 (OnClickedSlot, 이전 권능 칸 버튼 연결 그대로 동작). 빼기는 전체 초기화(LevelStatContainer.OnClickedReset)
// 씬에서 직접 만든 UI를 연결. 비운 칸은 건너뜀
public class LevelStatSlotPanel : MonoBehaviour
{
    [Header("표시")]
    public Image iconImage;          // 스탯 아이콘 (StatData.GetThum)
    public TMP_Text titleText;       // 스탯 이름
    [Tooltip("스탯 = 1포인트당 \"+1\" / 능력 = 설명 (번역 HeroUpgrade.{이름}.Desc)")]
    public TMP_Text descText;
    [Tooltip("찍은 포인트 수")]
    public TMP_Text pointText;
    [Tooltip("찍은 포인트로 오른 값 합계 \"+3\"")]
    public TMP_Text totalValueText;
    [Tooltip("포인트를 1 이상 찍었으면 켜짐")]
    public GameObject highlightObject;
    [UnityEngine.Serialization.FormerlySerializedAs("equiptingPanel")]
    [UnityEngine.Serialization.FormerlySerializedAs("equippedPanel")]
    [Tooltip("포인트를 1 이상 찍었으면 켜짐 (이전 권능 '선택됨' 표시)")]
    public GameObject selectedPanel;
    [Header("잠김 (묶음 해금 레벨 미달)")]
    public GameObject lockPanel;
    public TMP_Text lockText;        // "Lv.N"

    [HideInInspector] public int tierIdx;
    [HideInInspector] public int optionIdx;
    KingLevelStatSetting.StatOption option;

    public void SetOption(int tierIdx, int optionIdx, KingLevelStatSetting.Tier tier)
    {
        this.tierIdx = tierIdx;
        this.optionIdx = optionIdx;
        option = tier != null && optionIdx < tier.options.Length ? tier.options[optionIdx] : null;
        gameObject.SetActive(option != null);
        if (option == null)
            return;

        var user = UserDataManager.Instance.userKingSlimeManager;
        int points = user.GetPoints(tierIdx, optionIdx);
        bool unlocked = user.IsTierUnlocked(tierIdx);

        if (iconImage != null && option.kind == KingLevelStatSetting.OptionKind.Stat)
        {
            Sprite icon = StatData.GetThum(option.statType);
            if (icon != null)
                iconImage.sprite = icon;
        }
        if (titleText != null)
            titleText.text = option.Title;
        if (descText != null)
            descText.text = option.GetDesc(points); // 스탯 = 1포인트당 "+1", 능력 = 지금 포인트(0이면 1포인트) 값의 설명
        if (pointText != null)
            pointText.text = points.ToString();
        if (totalValueText != null)
            totalValueText.text = points > 0 ? option.GetValueText(points) : "";
        if (highlightObject != null)
            highlightObject.SetActive(points > 0);
        if (selectedPanel != null)
            selectedPanel.SetActive(points > 0);
        if (lockPanel != null)
            lockPanel.SetActive(!unlocked);
        if (lockText != null)
            lockText.text = $"Lv.{tier.unlockLevel}";
    }

    // 1포인트 찍기
    public void OnClickedSlot()
    {
        if (option == null)
            return;
        var tier = KingLevelStatSetting.Tiers[tierIdx];
        KingStatPointResult result = UserDataManager.Instance.userKingSlimeManager.TryAddPoint(tierIdx, optionIdx);
        switch (result)
        {
            case KingStatPointResult.Success:
                SoundManager.Play(SFXType.KingUpgrade);
                break;
            case KingStatPointResult.Locked:
                ToastCanvas.Toast(string.Format(TranslateManager.GetText("KingStat_Locked"), tier.unlockLevel));
                break;
            case KingStatPointResult.TierFull:
                SoundManager.Play(SFXType.Fail_NotEnough);
                ToastCanvas.Toast(string.Format(TranslateManager.GetText("KingStat_TierFull"), tier.maxPoints));
                break;
            case KingStatPointResult.NoPoint:
                SoundManager.Play(SFXType.Fail_NotEnough);
                ToastCanvas.Toast(TranslateManager.GetText("KingStat_NoPoint"));
                break;
        }
        Lobby.LobbyManager.Instance.GetLobbyCanvas(LobbyState.KingSlimeManagement).UpdateCanvas();
    }
}
