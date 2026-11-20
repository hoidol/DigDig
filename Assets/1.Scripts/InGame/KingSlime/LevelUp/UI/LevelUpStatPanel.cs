using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    AbilityData data;

    // nextLevel = 고르면 될 레벨 (1 = 새 능력)
    public void SetData(AbilityData data, int nextLevel)
    {
        this.data = data;
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
            categoryText.text = GetCategoryName(data.category);
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
            default: return "능력치";
        }
    }

    public void OnClickedSelect()
    {
        if (data != null && KingSlime.Instance != null)
            KingSlime.Instance.abilityInventory.Add(data);
        data = null; // 연타로 두 번 적용 방지
        InGame.LevelUpCanvas.Instance.CloseCanvas();
    }
}
