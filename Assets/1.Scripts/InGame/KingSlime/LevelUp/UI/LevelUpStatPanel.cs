using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// (2026-11-20) 레벨업 카드 한 장. AbilityData를 받아 분류·아이콘·이름·설명·레벨을 보여주고, 고르면 킹이 그 능력을 얻음
// 모양은 로비 패널(KSUpgradePanel·AuthoritySlotPanel)과 같은 계열: 어두운 바탕 + 흰 버튼 배경(슬라이스) + 아이콘 + 외곽선 글씨
// 카드 색(테두리·아이콘 바탕·분류 글씨) = 종류별 색, 종족은 종족 색(RaceKingAbility.tileColor)
public class LevelUpStatPanel : MonoBehaviour
{
    public TMP_Text titleText;       // 비우면 설명 위에 굵게
    public TMP_Text descriptionText;
    public TMP_Text levelText;       // 비워도 됨. 고르면 될 레벨 (Lv.N, 처음이면 NEW)
    public TMP_Text categoryText;    // 비워도 됨. 능력치 / 종족 …
    public Image thumImage;          // 비워도 됨
    public Image borderImage;        // 비워도 됨 (카드 색)
    public Image iconBgImage;        // 비워도 됨 (카드 색)
    [Header("종족 버프 칸 모양 (3×3, 왼위부터 가로 순서, 가운데 = 킹). 비우면 설명 글씨로 대신")]
    public GameObject shapeGrid;
    public Image[] shapeCells;
    public Color shapeEmptyColor = new Color(1f, 1f, 1f, 0.15f);
    [Header("설명 영역: 모양이 있을 때 / 없을 때 (위쪽 기준 y, 높이)")]
    public Vector2 descWithShape = new Vector2(-515, 140);
    public Vector2 descNoShape = new Vector2(-410, 245);

    public Color statColor = new Color(0.31f, 0.64f, 1f);
    public Color raceColor = new Color(1f, 0.78f, 0.29f);
    public Color jobColor = new Color(1f, 0.45f, 0.3f);
    public Color passiveColor = new Color(0.55f, 0.85f, 0.4f);
    public Color skillColor = new Color(0.75f, 0.45f, 1f);
    public Color expandColor = new Color(0.3f, 0.9f, 0.9f);
    public Color runeColor = new Color(0.45f, 0.95f, 0.55f);

    AbilityData data;
    Vector2Int? expandTile; // 버프 칸 확장 카드: 고르면 추가될 칸

    // 직업 카드: 버프가 걸릴 지금 버프 칸(종족 칸)을 직업 색으로 표시. 설명 = 직업 버프
    public void SetJobData(AbilityData data, IList<Vector2Int> currentTiles)
    {
        SetData(data, 1);
        DrawGrid(currentTiles, null, GetColor(data));
    }

    // 버프 칸 확장 카드: 지금 버프 칸(종족 색, 흐리게) + 추가될 칸(카드 색). 설명 끝에 칸 위치
    public void SetExpandData(AbilityData data, IList<Vector2Int> currentTiles, Vector2Int extraTile)
    {
        SetData(data, 1);
        expandTile = extraTile;
        descriptionText.text += $"\n추가 칸: {GetOffsetName(extraTile)}";
        Color raceCol = raceColor;
        if (KingSlime.Instance != null)
        {
            var race = KingSlime.Instance.abilityInventory.Get<RaceKingAbility>();
            if (race != null) { raceCol = race.tileColor; raceCol.a = 1f; }
        }
        DrawGrid(currentTiles, extraTile, raceCol * new Color(1f, 1f, 1f, 0.6f), GetColor(data));
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
        SetDescArea(true);
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
        SetShape(data.prefab as RaceKingAbility, color);
        if (borderImage != null)
            borderImage.color = color;
        if (iconBgImage != null)
            iconBgImage.color = color;
        if (categoryText != null)
        {
            categoryText.text = data.prefab is PassiveKingAbility passive
                ? $"패시브 · {(passive.kind == PassiveKind.Buff ? "버프형" : "공격형")}"
                : GetCategoryName(data.category);
            categoryText.color = color;
        }
    }

    // 종족이면 버프 칸 모양 표시 + 설명을 그 아래로
    void SetShape(RaceKingAbility race, Color color)
    {
        bool show = race != null && shapeGrid != null && shapeCells != null && shapeCells.Length == 9;
        if (shapeGrid != null)
            shapeGrid.SetActive(show);
        if (show)
        {
            for (int i = 0; i < 9; i++)
            {
                if (i == 4)
                    continue; // 가운데 = 킹 (프리팹 그림 그대로)
                var offset = new Vector2Int(i % 3 - 1, i / 3 - 1);
                shapeCells[i].color = System.Array.IndexOf(race.tileOffsets, offset) >= 0 ? color : shapeEmptyColor;
            }
        }
        SetDescArea(show);
    }

    void SetDescArea(bool show)
    {
        Vector2 area = show ? descWithShape : descNoShape;
        RectTransform rt = descriptionText.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, area.x);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, area.y);
    }

    Color GetColor(AbilityData data)
    {
        if (data.prefab is RaceKingAbility race)
        {
            Color c = race.tileColor;
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
            default: return statColor;
        }
    }

    // 번역 전 임시 글씨
    public static string GetCategoryName(AbilityCategory category)
    {
        switch (category)
        {
            case AbilityCategory.Race: return "종족";
            case AbilityCategory.Job: return "직업";
            case AbilityCategory.Passive: return "패시브";
            case AbilityCategory.ActiveSkill: return "스킬";
            case AbilityCategory.TileExpand: return "종족 강화";
            case AbilityCategory.Rune: return "룬";
            default: return "능력치";
        }
    }

    public void OnClickedSelect()
    {
        if (data != null && data.IsRune) // 룬 카드: 타일 선택으로 (레벨업은 타일을 고른 뒤 소모)
        {
            InGame.LevelUpCanvas.Instance.BeginRunePick(data.runeItem); // data는 남겨 둠 (취소하면 같은 카드로 돌아옴)
            return;
        }
        if (data != null && KingSlime.Instance != null)
        {
            Vector2Int? extra = expandTile;
            KingSlime.Instance.abilityInventory.Add(data, a =>
            {
                if (a is BuffTileExpandKingAbility expand && extra.HasValue)
                    expand.extraOffset = extra.Value;
            });
        }
        data = null; // 연타로 두 번 적용 방지
        InGame.LevelUpCanvas.Instance.CloseCanvas();
    }
}
