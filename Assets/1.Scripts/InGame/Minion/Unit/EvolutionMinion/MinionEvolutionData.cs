using System;
using System.Collections.Generic;
using UnityEngine;

// (2026-11-24) 5성 진화 갈래 하나. MinionData.evolutions(에셋 인스펙터)에 종마다 2개 → 5성이 되면 둘 중 하나를 고름 (MinionEvolutionCanvas)
// 이름·설명 = 번역 Evolution.{key} / Evolution.{key}.Desc ({0} 공격력 배율, {1} 공속 배율, {2} 사거리 증가)
// 갈래가 하나면 고르지 않고 바로 진화. 종에 갈래를 안 넣으면 DefaultBranches(임시 공용 2갈래)
[Serializable]
public class MinionEvolutionData
{
    public string key;
    [Tooltip("공격력 배율 (1 = 그대로)")] public float attackPowerMul = 1f;
    [Tooltip("공속 배율 (1 = 그대로)")] public float attackSpeedMul = 1f;
    [Tooltip("사거리 증가")] public float attackRangeAdd;
    [Tooltip("갈래 특수 동작 (선택). MinionEvolution 컴포넌트가 붙은 프리팹")]
    public MinionEvolution prefab;

    public string Title => TranslateManager.GetText($"Evolution.{key}");
    public string Desc => string.Format(TranslateManager.GetText($"Evolution.{key}.Desc"),
        Mathf.RoundToInt((attackPowerMul - 1f) * 100f), Mathf.RoundToInt((attackSpeedMul - 1f) * 100f), attackRangeAdd);

    // 임시 공용 2갈래 (종별 갈래가 정해지기 전): 힘 = 공격력 크게 / 신속 = 공속 크게
    public static readonly List<MinionEvolutionData> DefaultBranches = new List<MinionEvolutionData>
    {
        new MinionEvolutionData { key = "Power", attackPowerMul = 2f },
        new MinionEvolutionData { key = "Swift", attackSpeedMul = 1.7f },
    };
}
