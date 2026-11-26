using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

// (2026-11-20) 레벨업 카드 한 장. AbilityData를 받아 분류·아이콘·이름·설명·레벨을 보여주고, 고르면 킹이 그 능력을 얻음
// 모양은 로비 패널(이전 KSUpgradePanel·AuthoritySlotPanel)과 같은 계열: 어두운 바탕 + 흰 버튼 배경(슬라이스) + 아이콘 + 외곽선 글씨
// 카드 색(테두리·아이콘 바탕·분류 글씨) = 종류별 색, 용사는 용사 색(HeroKingAbility.tileColor)
// (2026-11-26) 카드 템플릿 = 가로로 긴 카드(왼쪽 아이콘 + 오른쪽 이름·설명)를 세로로 쌓음, 선택 버튼 없이 카드 어디를 눌러도 선택(OnPointerClick)
//   글씨·그림 위치는 씬 템플릿 그대로 (코드가 옮기지 않음)
public class LevelUpStatPanel : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text titleText;       // 비우면 설명 위에 굵게
    public TMP_Text descriptionText;
    public TMP_Text levelText;       // 비워도 됨. 고르면 될 레벨 (Lv.N, 처음이면 NEW)
    public TMP_Text categoryText;    // 비워도 됨. 능력치 / 종족 … (비우면 행운·대박 표시는 제목 앞에)
    public Image thumImage;          // 비워도 됨
    public Image borderImage;        // 비워도 됨 (카드 색)
    public Image iconBgImage;        // 비워도 됨 (카드 색)
    [Header("종족 버프 칸 모양 (3×3, 왼위부터 가로 순서, 가운데 = 킹). 비우면 설명 글씨로 대신")]
    public GameObject shapeGrid;
    public Image[] shapeCells;
    public Color shapeEmptyColor = new Color(1f, 1f, 1f, 0.15f);

    public Color statColor = new Color(0.31f, 0.64f, 1f);
    public Color raceColor = new Color(1f, 0.78f, 0.29f);
    public Color jobColor = new Color(1f, 0.45f, 0.3f);
    public Color passiveColor = new Color(0.55f, 0.85f, 0.4f);
    public Color skillColor = new Color(0.75f, 0.45f, 1f);
    public Color expandColor = new Color(0.3f, 0.9f, 0.9f);
    public Color runeColor = new Color(0.45f, 0.95f, 0.55f);

    public Color energyBoostColor = new Color(0.55f, 0.85f, 1f);

    AbilityData data;
    public AbilityData Data => data; // 지금 보여 주는 카드 (테스트)
    Vector2Int? expandTile; // 버프 칸 확장 카드: 고르면 추가될 칸
    Vector2Int[] heroTiles; // (2026-11-24) 용사 카드: 고르면 정예병 칸이 될 랜덤 2칸
    HeroKingAbility skillHero; // (2026-11-24) 스킬 해방 카드: 고르면 이 용사의 다음 스킬 해방

    // (2026-11-24) 용사 선택 카드: 정예병 칸 = 카드마다 랜덤 2칸 (그림·설명에 표시)
    public void SetHeroData(AbilityData data, Vector2Int[] tiles)
    {
        SetData(data, 1);
        heroTiles = tiles;
        SetShape(tiles, GetColor(data));
        var names = new List<string>();
        foreach (var t in tiles)
            names.Add(GetOffsetName(t));
        descriptionText.text += $"\n정예병 칸: {string.Join(", ", names)}";
    }

    // (2026-11-24) 스킬 해방 카드: 그 단계 스킬 이름·설명. (2026-11-26) 스킬 해방은 카드 없이 스탯 변화 화면에서 자동 → 지금은 안 씀
    public void SetHeroSkillData(HeroKingAbility hero, int skillIdx)
    {
        SetData(hero.data, hero.level);
        skillHero = hero;
        string title = hero.GetSkillName(skillIdx);
        string desc = hero.GetSkillDesc(skillIdx);
        if (skillIdx == 0) // (2026-11-25) 에너지는 1단계 해방 때 최대 2개, 이후는 용사 강화 카드로만
            desc += $"\n\n<color=#{ColorUtility.ToHtmlStringRGB(hero.energyColor)}>{hero.energyName}</color> 최대 {GameSetting.HERO_ENERGY_BASE_MAX}개 (하단 스킬 버튼으로 사용)";
        if (titleText != null)
        {
            titleText.text = title;
            descriptionText.text = desc;
        }
        else
            descriptionText.text = $"<size=120%><b>{title}</b></size>\n{desc}";
        if (levelText != null)
            levelText.text = "NEW";
        if (categoryText != null)
            categoryText.text = skillIdx == HeroKingAbility.COMMON_SKILL_INDEX ? "3단계 스킬 · 공용" : $"{skillIdx + 1}단계 스킬";
        if (thumImage != null)
        {
            thumImage.sprite = hero.GetSkillIcon(skillIdx);
            thumImage.enabled = thumImage.sprite != null;
        }
        SetShape(null, Color.white);
    }

    // 버프 칸 확장 카드: 지금 버프 칸(종족 색, 흐리게) + 추가될 칸(카드 색). 설명 끝에 칸 위치
    public void SetExpandData(AbilityData data, IList<Vector2Int> currentTiles, Vector2Int extraTile, int nextLevel = 1)
    {
        SetData(data, nextLevel);
        expandTile = extraTile;
        descriptionText.text += $"\n추가 칸: {GetOffsetName(extraTile)}";
        Color raceCol = raceColor;
        if (KingSlime.Instance != null)
        {
            var hero = KingSlime.Instance.abilityInventory.Get<HeroKingAbility>();
            if (hero != null) { raceCol = hero.tileColor; raceCol.a = 1f; }
        }
        DrawGrid(currentTiles, extraTile, raceCol * new Color(1f, 1f, 1f, 0.6f), GetColor(data));
        SetIconVisible(false); // (2026-11-26) 정예 칸 추가 카드 = 아이콘 대신 칸 그림(ShapeGrid)만
    }

    // 아이콘 바탕(IconBg, 아이콘·레벨 배지 포함) 켜기/끄기. 씬에서 꺼 둔 채 저장해도 일반 카드에선 켬
    void SetIconVisible(bool visible)
    {
        GameObject icon = iconBgImage != null ? iconBgImage.gameObject : thumImage != null ? thumImage.gameObject : null;
        if (icon != null && icon != gameObject)
            icon.SetActive(visible);
    }

    void DrawGrid(IList<Vector2Int> tiles, Vector2Int? extra, Color tileColor, Color extraColor = default)
    {
        bool show = shapeGrid != null && shapeCells != null && shapeCells.Length == 9;
        if (!show)
            return;
        shapeGrid.SetActive(true);
        for (int i = 0; i < 9; i++)
        {
            if (i == 4)
                continue;
            var offset = new Vector2Int(i % 3 - 1, i / 3 - 1);
            shapeCells[i].color = extra.HasValue && offset == extra.Value ? extraColor
                : tiles != null && tiles.Contains(offset) ? tileColor
                : shapeEmptyColor;
        }
    }

    static string GetOffsetName(Vector2Int o)
    {
        string v = o.y < 0 ? "위" : o.y > 0 ? "아래" : "";
        string h = o.x < 0 ? "왼" : o.x > 0 ? "오른" : "";
        if (h.Length > 0 && v.Length > 0)
            return $"{h}쪽 {v}";
        return h.Length > 0 ? $"{h}쪽" : v;
    }

    // nextLevel = 고르면 될 레벨 (1 = 새 능력)
    public void SetData(AbilityData data, int nextLevel)
    {
        this.data = data;
        expandTile = null;
        SetIconVisible(true);
        heroTiles = null;
        skillHero = null;
        string desc = data.GetDescription(nextLevel);
        if (titleText != null)
        {
            titleText.text = data.title;
            descriptionText.text = desc;
        }
        else // 제목 글씨가 없는 패널 → 설명 위에 굵게
            descriptionText.text = $"<size=120%><b>{data.title}</b></size>\n{desc}";

        if (levelText != null)
            levelText.text = nextLevel <= 1 ? "NEW" : $"Lv.{nextLevel}";
        if (thumImage != null)
        {
            thumImage.sprite = data.thum;
            thumImage.enabled = data.thum != null;
            thumImage.preserveAspect = true;
        }

        Color color = GetColor(data);
        // 버프 칸 모양 = 용사(또는 이전 종족) 칸
        // (2026-11-24) 이미 고른 용사(정예병 버프 강화 카드)는 실제 정예병 칸
        HeroKingAbility ownedHero = KingSlime.Instance != null && KingSlime.Instance.abilityInventory != null
            ? KingSlime.Instance.abilityInventory.Get(data) as HeroKingAbility : null;
        Vector2Int[] shape = ownedHero != null ? ownedHero.eliteOffsets
            : data.prefab is HeroKingAbility heroShape ? heroShape.eliteOffsets : null;
        SetShape(shape, color);
        if (borderImage != null)
            borderImage.color = color;
        if (iconBgImage != null)
            iconBgImage.color = color;
        if (categoryText != null)
        {
            categoryText.text = data.IsLuckyEnhance ? GetLuckyLabel(data) : GetCategoryName(data.category);
            categoryText.color = color;
        }
        else if (data.IsLuckyEnhance) // 분류 글씨가 없는 카드 → 행운·대박은 제목 앞에 카드 색으로 (×2·×3 카드인지 꼭 보이게)
        {
            string label = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{(data.minionAmount >= MinionEnhanceSystem.JACKPOT_AMOUNT ? "대박!" : "행운!")}</color> ";
            if (titleText != null)
                titleText.text = label + titleText.text;
            else
                descriptionText.text = label + descriptionText.text;
        }
    }

    static string GetLuckyLabel(AbilityData data) =>
        data.minionAmount >= MinionEnhanceSystem.JACKPOT_AMOUNT ? $"대박! 강화 ×{data.minionAmount}" : $"행운! 강화 ×{data.minionAmount}";

    // 종족이면 버프 칸 모양 표시 + 설명을 그 아래로
    void SetShape(Vector2Int[] offsets, Color color)
    {
        bool show = offsets != null && shapeGrid != null && shapeCells != null && shapeCells.Length == 9;
        if (shapeGrid != null)
            shapeGrid.SetActive(show);
        if (show)
        {
            for (int i = 0; i < 9; i++)
            {
                if (i == 4)
                    continue; // 가운데 = 킹 (프리팹 그림 그대로)
                var offset = new Vector2Int(i % 3 - 1, i / 3 - 1);
                shapeCells[i].color = System.Array.IndexOf(offsets, offset) >= 0 ? color : shapeEmptyColor;
            }
        }
    }

    public Color luckyColor = new Color(1f, 0.82f, 0.2f); // (2026-11-25) 행운 미니언 강화 카드 (×2) 금색
    public Color jackpotColor = new Color(1f, 0.35f, 0.85f); // (2026-11-25) 대박 미니언 강화 카드 (×3) 분홍

    Color GetColor(AbilityData data)
    {
        if (data.IsLuckyEnhance)
            return data.minionAmount >= MinionEnhanceSystem.JACKPOT_AMOUNT ? jackpotColor : luckyColor;
        if (data.prefab is HeroKingAbility hero)
        {
            Color c = hero.tileColor;
            c.a = 1f;
            return c;
        }
        switch (data.category)
        {
            case AbilityCategory.Race: return raceColor;
            case AbilityCategory.Job: return jobColor;
            case AbilityCategory.Passive: return passiveColor;
            case AbilityCategory.ActiveSkill: return skillColor;
            case AbilityCategory.TileExpand: return expandColor;
            case AbilityCategory.Rune: return runeColor;
            case AbilityCategory.HeroBoost: return energyBoostColor;
            default: return statColor;
        }
    }

    // 번역 전 임시 글씨
    public static string GetCategoryName(AbilityCategory category)
    {
        switch (category)
        {
            case AbilityCategory.Hero: return "용사";
            case AbilityCategory.Race: return "종족";
            case AbilityCategory.Job: return "직업";
            case AbilityCategory.Passive: return "패시브";
            case AbilityCategory.ActiveSkill: return "스킬";
            case AbilityCategory.TileExpand: return "정예 칸 추가";
            case AbilityCategory.Rune: return "룬";
            case AbilityCategory.HeroBoost: return "용사 강화";
            case AbilityCategory.MinionEnhance: return "미니언 강화";
            default: return "능력치";
        }
    }

    // (2026-11-26) 카드 어디를 눌러도 선택 (선택 버튼 삭제). 튀어나오는 중(크기 0.5 미만)엔 무시 → 창이 뜨자마자 잘못 누르는 것 방지
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging || transform.localScale.x < 0.5f)
            return;
        OnClickedSelect();
    }

    public void OnClickedSelect()
    {
        if (data != null && data.IsMinionEnhance) // (2026-11-25) 미니언 강화 카드
        {
            MinionEnhanceSystem.Add(data.minionKey, data.minionStat, data.minionAmount);
            data = null;
            InGame.LevelUpCanvas.Instance.CloseCanvas();
            return;
        }
        if (skillHero != null) // (2026-11-24) 스킬 해방
        {
            skillHero.UnlockNextSkill();
            skillHero = null;
        }
        else if (data != null && KingSlime.Instance != null)
        {
            Vector2Int? extra = expandTile;
            Vector2Int[] tiles = heroTiles;
            KingSlime.Instance.abilityInventory.Add(data, a =>
            {
                if (a is EliteAreaExpandKingAbility expand && extra.HasValue)
                    expand.extraOffset = extra.Value;
                if (a is HeroKingAbility hero && tiles != null && hero.level == 1) // 새로 고른 용사만 (강화는 칸 그대로)
                    hero.eliteOffsets = tiles;
            });
        }
        data = null; // 연타로 두 번 적용 방지
        InGame.LevelUpCanvas.Instance.CloseCanvas();
    }
}
