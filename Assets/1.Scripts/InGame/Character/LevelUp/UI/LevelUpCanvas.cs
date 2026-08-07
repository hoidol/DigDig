using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class LevelUpCanvas : CanvasUI<LevelUpCanvas>
{
    const int SELECT_COUNT = 3;
    const float LOW_HP_THRESHOLD = 30f;
    const float FULL_HEAL_APPEAR_CHANCE = 0.7f;

    public LevelUpStatPanel[] levelUpStatPanels;
    //뽑기
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        var selected = PickBonusPanels();

        int i = 0;
        foreach (var panel in levelUpStatPanels)
        {
            bool isSelected = selected.Contains(panel);
            panel.gameObject.SetActive(isSelected);
            if (isSelected)
            {
                panel.SetLevelUpStatPanel();
                RectTransform rTr = panel.GetComponent<RectTransform>();
                rTr.DOKill();
                rTr.localScale = new Vector3(0, 0, 0);
                rTr.DOScale(1.1f, 0.2f + i * 0.15f).SetUpdate(true).OnComplete(() =>
                {
                    rTr.DOScale(1, 0.25f).SetUpdate(true);
                });
                i++;
            }

        }
    }

    List<LevelUpStatPanel> PickBonusPanels()
    {
        var pool = levelUpStatPanels.ToList();
        var result = new List<LevelUpStatPanel>();

        bool isLowHp = Character.Instance.curHp < LOW_HP_THRESHOLD;
        if (isLowHp && UnityEngine.Random.value < FULL_HEAL_APPEAR_CHANCE)
        {
            var fullHealPanel = pool.FirstOrDefault(p => p.levelUpStatType == LevelUpStatType.FullHeal);
            if (fullHealPanel != null)
            {
                result.Add(fullHealPanel);
                pool.Remove(fullHealPanel);
            }
        }

        while (result.Count < SELECT_COUNT && pool.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}