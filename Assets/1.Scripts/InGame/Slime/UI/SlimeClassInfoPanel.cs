using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-21) 유닛 정보 패널(AllySettingPanel)에서 슬라임을 선택했을 때: 공격 타입 칸 옆에 현재 종족·직업 칸 + 설명 칸에
//   "종족 이름 : 설명" / "직업 이름 : 설명" (설명 칸은 세로 스크롤)
// 아직 고르지 않았으면 그 칸은 흐리게(disabledAlpha) + "종족"/"직업" 글씨만
// 미니언을 선택하면 종족·직업 칸은 숨기고 이름 글씨(nameText)를 다시 보임
public class SlimeClassInfoPanel : MonoBehaviour
{
    [Header("종족 칸")]
    public CanvasGroup raceBadge;
    public Image raceIcon;
    public TMP_Text raceText;
    [Header("직업 칸")]
    public CanvasGroup jobBadge;
    public Image jobIcon;
    public TMP_Text jobText;
    [Header("공용")]
    public TMP_Text nameText;     // 미니언 이름 (슬라임이면 숨김, 종족·직업 칸이 그 자리)
    public TMP_Text descText;     // 설명 (미니언 설명과 같은 글씨)
    public ScrollRect descScroll; // 설명 스크롤
    public float disabledAlpha = 0.35f;

    // (2026-11-23) 종족·직업 → 용사 하나: 첫 칸(raceBadge) = 용사, 직업 칸은 숨김. 설명 = 용사 설명 + 에너지
    public void ShowSlime(Slime slime)
    {
        SlimeAbilityInventory inv = slime != null ? slime.abilityInventory : null;
        HeroSlimeAbility hero = inv?.Get<HeroSlimeAbility>();

        SetBadge(raceBadge, raceIcon, raceText, hero, "용사");
        if (jobBadge != null)
            jobBadge.gameObject.SetActive(false);
        if (nameText != null)
            nameText.gameObject.SetActive(false);

        if (descText != null)
        {
            string line = GetLine(hero, "용사", $"Lv.{LevelUpChoiceRule.HERO_LEVEL}에 선택");
            if (hero != null) // (2026-11-24) 에너지 개수 + 스킬 해방 상황
            {
                string col = ColorUtility.ToHtmlStringRGB(hero.energyColor);
                line += $"\n<color=#{col}>{hero.energyName} {hero.Charges}/{hero.MaxCharges}</color>";
                for (int s = 0; s < HeroSlimeAbility.SKILL_COUNT; s++)
                    line += hero.IsSkillUnlocked(s)
                        ? $"\n<b>{hero.GetSkillName(s)}</b> : {hero.GetSkillDesc(s)}"
                        : $"\n<color=#9AA0A6>{hero.GetSkillName(s)} : Lv.{LevelUpChoiceRule.GetSkillUnlockLevel(s)}에 해방</color>";
            }
            descText.text = line;
        }
        ResetScroll();
    }

    public void ShowMinion()
    {
        if (raceBadge != null)
            raceBadge.gameObject.SetActive(false);
        if (jobBadge != null)
            jobBadge.gameObject.SetActive(false);
        if (nameText != null)
            nameText.gameObject.SetActive(true);
        ResetScroll();
    }

    void SetBadge(CanvasGroup badge, Image icon, TMP_Text text, SlimeAbility ability, string emptyName)
    {
        if (badge == null)
            return;
        badge.gameObject.SetActive(true);
        bool has = ability != null && ability.data != null;
        badge.alpha = has ? 1f : disabledAlpha;
        if (text != null)
            text.text = has ? ability.data.title : emptyName;
        if (icon != null)
        {
            icon.sprite = has ? ability.data.thum : null;
            icon.enabled = has && ability.data.thum != null;
            icon.preserveAspect = true;
        }
    }

    // "오크 : 버프 칸 미니언·슬라임: 공격력 +10%" / 아직 없으면 "종족 : Lv.1에 선택"
    static string GetLine(SlimeAbility ability, string emptyName, string emptyDesc)
    {
        if (ability == null || ability.data == null)
            return $"<color=#9AA0A6>{emptyName} : {emptyDesc}</color>";
        string desc = ability.data.GetDescription(ability.level);
        return $"<b>{ability.data.title}</b> : {desc}";
    }

    // 글씨 길이가 바뀌면 스크롤 내용 크기를 다시 계산하고 맨 위로 (패널이 켜진 뒤에 해야 해서 LateUpdate에서)
    bool needsScrollReset;
    void ResetScroll() => needsScrollReset = true;

    void LateUpdate()
    {
        if (!needsScrollReset)
            return;
        needsScrollReset = false;
        if (descScroll == null || descScroll.content == null)
            return;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(descScroll.content);
        descScroll.verticalNormalizedPosition = 1f;
    }
}
