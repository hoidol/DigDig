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

    // (2026-11-20) 킹 레벨업 → 창을 띄우고 시간 정지, 닫으면 열기 전 timeScale로 복구.
    // 한 번에 여러 레벨이 오르면 닫을 때마다 남은 만큼 다시 열림 (그동안 시간은 계속 멈춤)
    static int pendingCount;
    float prevTimeScale = 1f;
    bool isPausing;

    public static void RequestOpen()
    {
        if (Instance == null)
            return;
        pendingCount++;
        if (!Instance.gameObject.activeSelf)
            Instance.OpenCanvas();
    }

    void OnDestroy()
    {
        pendingCount = 0;
        if (isPausing)
            Time.timeScale = prevTimeScale;
    }

    public override void CloseCanvas()
    {
        base.CloseCanvas();
        pendingCount = Mathf.Max(0, pendingCount - 1);
        if (pendingCount > 0)
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
        if (!isPausing)
        {
            isPausing = true;
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        base.OpenCanvas(closeCallback);

        // 카드 구성 = LevelUpChoiceRule (종족이 없으면 종족, 그 외 킹 능력치. 최대 레벨 제외)
        KingSlime king = KingSlime.Instance;
        KingGrowthSetting setting = KingGrowthSetting.Instance;
        List<AbilityData> choices = king != null && setting != null
            ? LevelUpChoiceRule.GetChoices(king.lv, king.abilityInventory.HasCategory(AbilityCategory.Race),
                setting.raceAbilities, setting.statAbilities, king.abilityInventory.GetLevel, rng)
            : new List<AbilityData>();

        EnsurePanels(choices.Count);
        if (headerText != null)
            headerText.text = king != null ? $"레벨 업! Lv.{king.lv}" : "레벨 업!";
        if (subText != null)
            subText.text = choices.Count > 0 && choices[0].category == AbilityCategory.Race
                ? "종족을 선택하세요" : "능력을 선택하세요";
        int i = 0;
        foreach (var panel in levelUpStatPanels)
        {
            bool active = i < choices.Count;
            panel.gameObject.SetActive(active);
            if (!active)
                continue;
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
