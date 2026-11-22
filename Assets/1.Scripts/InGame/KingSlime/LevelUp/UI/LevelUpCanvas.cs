using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
namespace InGame
{
    public class LevelUpCanvas : CanvasUI<LevelUpCanvas>
{
    public LevelUpStatPanel[] levelUpStatPanels;
    public TMPro.TMP_Text headerText; // 비워도 됨. "레벨 업!"
    public TMPro.TMP_Text subText;    // 비워도 됨. 무엇을 고르는지 (종족을 선택하세요 …)

    // (2026-11-21) 레벨업 창은 전투 중엔 안 열림. (2026-11-22) 경험치가 다 차면 바로 OpenPending (KingSlime.AddExp, 레벨업 버튼 삭제)
    // 밀린 레벨업(KingSlime.pendingLevelUps)만큼 연속으로 열림: 하나 고르면 닫히고 남았으면 다시 열림. (2026-11-22) 시간은 멈추지 않음
    float prevTimeScale = 1f;
    bool isPausing;

    // (2026-11-21) 룬 카드를 고르고 타일을 고르는 중 (창은 숨기고 시간 정지 유지, 레벨업 버튼·Next 숨김)
    public static bool IsPickingRune { get; private set; }

    public void BeginRunePick(ItemData item)
    {
        IsPickingRune = true;
        gameObject.SetActive(false); // CloseCanvas(레벨업 소모)는 타일을 고른 뒤에
        SelectItemCanvas.Instance.OpenRuneTilePick(item, FinishRunePick);
    }

    // 타일을 고르지 않고 닫음 → 같은 카드 화면으로 돌아감
    public void CancelRunePick()
    {
        IsPickingRune = false;
        gameObject.SetActive(true);
    }

    void FinishRunePick()
    {
        IsPickingRune = false;
        KingSlime.Instance?.ConsumeLevelUp();
        if (KingSlime.Instance != null && KingSlime.Instance.pendingLevelUps > 0)
        {
            OpenCanvas();
            return;
        }
        if (isPausing)
        {
            isPausing = false;
            Time.timeScale = prevTimeScale;
        }
    }

    public static bool CanOpen => !IsPickingRune && Instance != null && KingSlime.Instance != null && KingSlime.Instance.pendingLevelUps > 0
        && GameManager.Instance != null && GameManager.Instance.isBreakTime;

    public static void OpenPending()
    {
        if (!CanOpen || Instance.gameObject.activeSelf)
            return;
        Instance.OpenCanvas();
    }

    void OnDestroy()
    {
        IsPickingRune = false;
        if (isPausing)
            Time.timeScale = prevTimeScale;
    }

    public override void CloseCanvas()
    {
        base.CloseCanvas();
        KingSlime.Instance?.ConsumeLevelUp();
        if (KingSlime.Instance != null && KingSlime.Instance.pendingLevelUps > 0)
        {
            OpenCanvas();
            return;
        }
        if (isPausing)
        {
            isPausing = false;
            Time.timeScale = prevTimeScale;
        }
    }

    //뽑기
    public override void OpenCanvas(Action closeCallback = null)
    {
        // (2026-11-22) 시간은 멈추지 않음 (브레이크타임에만 열려서 멈출 필요 없음, 룬 타일 선택도 마찬가지)
        base.OpenCanvas(closeCallback);

        // 카드 구성 = LevelUpChoiceRule (종족 → Lv.5 직업 → Lv.10 버프 칸 확장, 그 외 킹 능력치. 최대 레벨 제외)
        KingSlime king = KingSlime.Instance;
        KingGrowthSetting setting = KingGrowthSetting.Instance;
        List<AbilityData> choices = king != null && setting != null
            ? LevelUpChoiceRule.GetChoices(MakeChoiceInput(king, setting), rng)
            : new List<AbilityData>();

        // 버프 칸 확장 카드: 카드마다 추가 칸 (지금 버프 칸을 뺀 나머지 중 랜덤, 카드끼리 다르게)
        List<Vector2Int> currentTiles = king != null ? king.abilityInventory.GetBuffTileOffsets() : new List<Vector2Int>();
        List<Vector2Int> expandTiles = LevelUpChoiceRule.PickExpandTiles(currentTiles, choices.Count, rng);

        EnsurePanels(choices.Count);
        if (headerText != null)
            headerText.text = king != null ? $"레벨 업! Lv.{king.ChoiceLevel}" : "레벨 업!";
        if (subText != null)
        {
            // 모든 카드가 같은 종류일 때만 그 종류 안내 (능력치 카드 사이에 섞인 패시브 강화 카드는 "능력")
            AbilityCategory category = choices.Count > 0 && choices.All(c => c.category == choices[0].category)
                ? choices[0].category : AbilityCategory.Stat;
            subText.text = category == AbilityCategory.Race ? "종족을 선택하세요"
                : category == AbilityCategory.Job ? "직업을 선택하세요"
                : category == AbilityCategory.TileExpand ? "추가할 버프 칸을 선택하세요"
                : category == AbilityCategory.Passive ? "패시브를 선택하세요"
                : "능력을 선택하세요";
        }
        int i = 0;
        foreach (var panel in levelUpStatPanels)
        {
            bool active = i < choices.Count;
            panel.gameObject.SetActive(active);
            if (!active)
                continue;
            if (choices[i].category == AbilityCategory.TileExpand && i < expandTiles.Count)
                panel.SetExpandData(choices[i], currentTiles, expandTiles[i]);
            else if (choices[i].category == AbilityCategory.Job)
                panel.SetJobData(choices[i], currentTiles);
            else
                panel.SetData(choices[i], king.abilityInventory.GetLevel(choices[i]) + 1);
            RectTransform rTr = panel.GetComponent<RectTransform>();
            rTr.DOKill();
            rTr.localScale = Vector3.zero;
            rTr.DOScale(1.1f, 0.2f + i * 0.15f).SetUpdate(true).OnComplete(() =>
            {
                rTr.DOScale(1, 0.25f).SetUpdate(true);
            });
            i++;
        }

        // 고를 카드가 없으면(모두 최대 레벨) 그냥 닫음
        if (choices.Count == 0)
            CloseCanvas();
    }

    static readonly System.Random rng = new System.Random();

    // 레벨업 룬 카드 (KingGrowthSetting.runeKeys 룬마다 한 장)
    static List<AbilityData> GetRuneCards()
    {
        var list = new List<AbilityData>();
        if (ItemManager.Instance == null)
            return list;
        KingGrowthSetting setting = KingGrowthSetting.Instance;
        foreach (ItemData item in ItemManager.Instance.GetLevelUpRuneItems(setting != null ? setting.runeKeys : null))
            list.Add(AbilityData.CreateRuneCard(item));
        return list;
    }

    // 다음 레벨업 창의 카드 구성 입력 (ExpBar 버튼 글씨에도 사용)
    public static LevelUpChoiceRule.Input MakeChoiceInput(KingSlime king, KingGrowthSetting setting)
    {
        KingAbilityInventory inv = king.abilityInventory;
        RaceKingAbility race = inv.Get<RaceKingAbility>();
        PassiveKingAbility passive = inv.Get<PassiveKingAbility>();
        return new LevelUpChoiceRule.Input
        {
            level = king.ChoiceLevel,
            hasRace = race != null,
            hasJob = inv.HasCategory(AbilityCategory.Job),
            hasTileExpand = inv.HasCategory(AbilityCategory.TileExpand),
            hasPassive = passive != null,
            racePool = setting.raceAbilities,
            jobPool = setting.jobAbilities,
            tileExpand = setting.tileExpandAbility,
            passivePool = race != null ? setting.GetRacePassives(race.raceType) : null,
            statPool = GetRuneCards(), // (2026-11-21) 능력치 카드 대신 룬 카드 (KingGrowthSetting.statAbilities는 지금 안 씀)
            ownedPassive = passive != null ? passive.data : null,
            getLevel = inv.GetLevel,
        };
    }

    // 패널은 씬에 1개만 두어도 됨: 빈 칸(null)은 빼고, 모자라면 첫 패널을 같은 부모 아래에 복사
    void EnsurePanels(int count)
    {
        var panels = (levelUpStatPanels ?? new LevelUpStatPanel[0]).Where(p => p != null).ToList();
        if (panels.Count == 0)
        {
            Debug.LogWarning("[LevelUpCanvas] 레벨업 패널 없음");
            levelUpStatPanels = panels.ToArray();
            return;
        }
        while (panels.Count < count)
        {
            LevelUpStatPanel copy = Instantiate(panels[0], panels[0].transform.parent);
            copy.name = $"{panels[0].name}_{panels.Count}";
            panels.Add(copy);
        }
        levelUpStatPanels = panels.ToArray();
    }
}
}
