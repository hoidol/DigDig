using System.Collections.Generic;
using TMPro;
using UnityEngine;

// (2026-11-27) 결과 화면 게임 기록: 통계 칸(이름 : 값) + 피해 순위(장착 미니언 + 킹, 진화·용사 스킬 피해 포함)
// 씬 = InGame ResultCanvas/Panel/Bg 하위 ResultStats·ResultDamage (메뉴 Tools/InGame/Add Result Stats To ResultCanvas)
// 줄은 씬의 템플릿 1개(statRowTemplate·damageRowTemplate)를 데이터 수만큼 복사 → 템플릿 모양을 고치면 전부 바뀜. 템플릿이 비어 있으면 경고만
public class ResultStatsPanel : MonoBehaviour
{
    public ResultStatRow statRowTemplate;
    public Transform statContainer;
    public DamageMeterSystemPanel damageRowTemplate;
    public Transform damageContainer;

    readonly List<ResultStatRow> statRows = new List<ResultStatRow>();
    readonly List<DamageMeterSystemPanel> damageRows = new List<DamageMeterSystemPanel>();

    // bestWave = 이번 판을 포함한 최고 기록 (표시 번호)
    public void Show(int bestWave)
    {
        ShowStats(GetStats(bestWave));
        ShowDamages(DamageMeterSystem.GetEntries());
    }

    public static List<(string label, string value)> GetStats(int bestWave)
    {
        int merge = MinionFieldController.Instance != null ? MinionFieldController.Instance.mergeCount : 0;
        KingSlime king = KingSlime.Instance;
        HeroKingAbility hero = king != null && king.abilityInventory != null ? king.abilityInventory.Get<HeroKingAbility>() : null;
        string kingName = hero != null && hero.data != null ? hero.data.title : TranslateManager.GetText("DamageMeter_King");
        return new List<(string, string)>
        {
            (T("Result_Stat_PlayTime"), GameResultStats.FormatTime(GameResultStats.PlayTime)),
            (T("Result_Stat_BestWave"), $"WAVE {bestWave}"),
            (T("Result_Stat_Kill"), GameResultStats.KillCount.ToString("N0")),
            (T("Result_Stat_StrongKill"), string.Format(T("Result_Stat_StrongKill_Value"), GameResultStats.EliteKillCount, GameResultStats.BossKillCount)),
            (T("Result_Stat_Leak"), GameResultStats.LeakCount.ToString("N0")),
            (T("Result_Stat_Card"), GameResultStats.PlacedCardCount.ToString("N0")),
            (T("Result_Stat_Merge"), $"{merge:N0} · {GameResultStats.EvolveCount:N0}"),
            (T("Result_Stat_King"), $"{kingName} Lv.{(king != null ? king.lv : 0)}"),
        };
    }

    static string T(string key) => TranslateManager.GetText(key);

    void ShowStats(List<(string label, string value)> stats)
    {
        if (statRowTemplate == null)
        {
            Debug.LogWarning("[ResultStatsPanel] statRowTemplate 없음 — 메뉴 Tools/InGame/Add Result Stats To ResultCanvas");
            return;
        }
        Fill(statRows, statRowTemplate, statContainer, stats.Count);
        for (int i = 0; i < stats.Count; i++)
            statRows[i].Set(stats[i].label, stats[i].value);
    }

    void ShowDamages(List<DamageMeterSystem.Entry> entries)
    {
        if (damageRowTemplate == null)
        {
            Debug.LogWarning("[ResultStatsPanel] damageRowTemplate 없음 — 메뉴 Tools/InGame/Add Result Stats To ResultCanvas");
            return;
        }
        Fill(damageRows, damageRowTemplate, damageContainer, entries.Count);
        double max = entries.Count > 0 ? entries[0].damage : 0;
        double total = DamageMeterSystem.AllDamage;
        for (int i = 0; i < entries.Count; i++)
            damageRows[i].SetData(entries[i], max, total);
    }

    // 템플릿(꺼 둠)을 count개만큼 복사해 켬, 남는 복사본은 끔
    static void Fill<T>(List<T> rows, T template, Transform container, int count) where T : Component
    {
        template.gameObject.SetActive(false);
        Transform parent = container != null ? container : template.transform.parent;
        while (rows.Count < count)
            rows.Add(Instantiate(template, parent));
        for (int i = 0; i < rows.Count; i++)
        {
            rows[i].gameObject.SetActive(i < count);
            rows[i].transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1 + i);
        }
    }
}
