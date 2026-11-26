using System;
using System.Collections.Generic;
using UnityEngine;

// (2026-11-24) 5성 진화 갈래 하나. 5성이 되면 진화 선택 창(MinionEvolutionCanvas)에서 고름
// (2026-11-26) 종마다 진화 1개 = MinionEvolutionSetting(Resources 에셋)의 Config (수치) + 이름·설명 = MinionData.csv EvolutionName / EvolutionDesc
//   갈래가 1개여도 선택 창을 띄워 눌러서 진화. MinionData.evolutions(에셋 인스펙터)는 예전 방식(스탯 배율 + prefab), Setting에 칸이 있으면 그쪽이 우선
[Serializable]
public class MinionEvolutionData
{
    public string key;
    [Tooltip("공격력 배율 (1 = 그대로)")] public float attackPowerMul = 1f;
    [Tooltip("공속 배율 (1 = 그대로)")] public float attackSpeedMul = 1f;
    [Tooltip("사거리 증가")] public float attackRangeAdd;
    [Tooltip("갈래 특수 동작 (선택). MinionEvolution 컴포넌트가 붙은 프리팹")]
    public MinionEvolution prefab;

    [NonSerialized] public EvolutionConfig config;   // (2026-11-26) Setting의 종별 수치·동작
    [NonSerialized] public MinionData minionData;    // 이름·설명을 읽을 미니언 데이터

    // (2026-11-26) 진화 이름·설명 번역 key (Translate.csv): Minion.{Key}.Evolution / Minion.{Key}.EvolutionDesc
    //   번역이 있으면 번역, 없으면 MinionData.csv EvolutionMinionName / EvolutionMinionDesc (한글 원본)
    public static string NameKey(string minionKey) => $"Minion.{minionKey}.Evolution";
    public static string DescKey(string minionKey) => $"Minion.{minionKey}.EvolutionDesc";

    public string Title
    {
        get
        {
            if (config != null && TranslateManager.HasKey(NameKey(key)))
                return TranslateManager.GetText(NameKey(key));
            if (minionData != null && !string.IsNullOrEmpty(minionData.evolutionName))
                return minionData.evolutionName;
            if (config != null && minionData != null)
                return string.Format(TranslateManager.GetText("Evolution_DefaultName"), minionData.Title);
            return TranslateManager.GetText($"Evolution.{key}");
        }
    }

    public string Desc
    {
        get
        {
            if (config != null)
            {
                string raw = TranslateManager.HasKey(DescKey(key)) ? TranslateManager.GetText(DescKey(key))
                    : minionData != null ? minionData.evolutionDesc : null;
                if (string.IsNullOrEmpty(raw))
                    return "";
                try { return string.Format(raw, config.GetDescArgs()); }
                catch (FormatException) { return raw; }
            }
            return string.Format(TranslateManager.GetText($"Evolution.{key}.Desc"),
                Mathf.RoundToInt((attackPowerMul - 1f) * 100f), Mathf.RoundToInt((attackSpeedMul - 1f) * 100f), attackRangeAdd);
        }
    }

    // Setting Config로 만든 갈래
    public static MinionEvolutionData FromConfig(MinionData data, EvolutionConfig config) => new MinionEvolutionData
    {
        key = data.key,
        attackPowerMul = config.attackPowerMul,
        attackSpeedMul = config.attackSpeedMul,
        config = config,
        minionData = data,
    };

    // (이전) 임시 공용 2갈래 — 2026-11-26부터 안 씀 (Setting에 칸이 없는 종은 진화 없음)
    public static readonly List<MinionEvolutionData> DefaultBranches = new List<MinionEvolutionData>
    {
        new MinionEvolutionData { key = "Power", attackPowerMul = 2f },
        new MinionEvolutionData { key = "Swift", attackSpeedMul = 1.7f },
    };
}
