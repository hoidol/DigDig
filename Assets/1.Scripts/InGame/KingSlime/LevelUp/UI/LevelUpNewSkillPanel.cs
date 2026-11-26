using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-26) 레벨업 스탯 변화 화면(StatChangeScreen) 안의 "새 스킬" 칸 (씬 LevelUpCanvas/Panel/Bg/StatChangeScreen/NewSkill)
// 스킬이 해방되는 레벨(Lv.2·6·10)에 어떤 스킬을 얻었는지 자세히: 머리 "1단계 스킬 개방!" + 스킬 아이콘 + 이름 + 설명(지금 수치) + 소모 에너지·위치 지정 여부
// 모양은 씬에서 직접 수정, 연결 칸이 빈 것은 건너뜀. 스킬 해방이 없는 레벨엔 숨김
public class LevelUpNewSkillPanel : MonoBehaviour
{
    [Tooltip("머리 글씨 \"1단계 스킬 개방!\"")]
    public TMP_Text headerText;
    [Tooltip("스킬 이름")]
    public TMP_Text titleText;
    [Tooltip("스킬 설명 + 소모 에너지 등")]
    public TMP_Text descText;
    [Tooltip("스킬 아이콘")]
    public Image thumImage;
    [Tooltip("아이콘 아래 배지 글씨 \"1단계\" / \"공용\"")]
    public TMP_Text levelText;
    [Tooltip("비워도 됨. 용사 에너지 색")]
    public Image borderImage;
    [Tooltip("비워도 됨. 용사 에너지 색")]
    public Image iconBgImage;

    public void Show(HeroKingAbility hero, int skillIdx)
    {
        if (hero == null || skillIdx < 0)
        {
            Hide();
            return;
        }
        gameObject.SetActive(true);
        bool common = skillIdx == HeroKingAbility.COMMON_SKILL_INDEX;
        string stageName = common ? "3단계 공용 스킬" : $"{skillIdx + 1}단계 스킬";
        if (headerText != null)
            headerText.text = $"{stageName} 개방!";
        if (titleText != null)
            titleText.text = hero.GetSkillName(skillIdx);
        if (levelText != null)
            levelText.text = common ? "공용" : $"{skillIdx + 1}단계";
        if (thumImage != null)
        {
            thumImage.sprite = hero.GetSkillIcon(skillIdx);
            thumImage.enabled = thumImage.sprite != null;
            thumImage.preserveAspect = true;
        }
        Color color = hero.energyColor;
        color.a = 1f;
        if (borderImage != null)
            borderImage.color = color;
        if (iconBgImage != null)
            iconBgImage.color = color;
        if (descText != null)
            descText.text = GetDetailText(hero, skillIdx);

        RectTransform rt = (RectTransform)transform;
        rt.DOKill();
        rt.localScale = Vector3.one * 0.6f;
        rt.DOScale(1f, 0.35f).SetDelay(0.15f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void Hide() => gameObject.SetActive(false);

    // 설명(지금 수치) + 소모 에너지 + 사용 방법. 1단계 해방이면 에너지 최대 개수 안내
    public static string GetDetailText(HeroKingAbility hero, int skillIdx)
    {
        string energy = $"<color=#{ColorUtility.ToHtmlStringRGB(hero.energyColor)}>{hero.energyName}</color>";
        string text = hero.GetSkillDesc(skillIdx);
        text += $"\n<size=85%>소모 {energy} {hero.GetSkillCost(skillIdx)}개"
            + (skillIdx != HeroKingAbility.COMMON_SKILL_INDEX && hero.NeedsTargetPosition(skillIdx) ? " · 버튼을 누른 뒤 원하는 위치를 눌러 사용" : " · 하단 스킬 버튼으로 사용")
            + "</size>";
        if (skillIdx == 0) // 에너지는 1단계 해방 때 최대 2개
            text += $"\n<size=85%>{energy} 최대 {GameSetting.HERO_ENERGY_BASE_MAX}개 (wave 중 충전)</size>";
        return text;
    }
}
