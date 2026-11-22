using System;
using System.Collections.Generic;
using UnityEngine;

// 공격 속성(AttackType) × 방어 타입(DefenceType) 피해 배율 표
// Resources/DefenceInfo 에셋 하나를 게임 전역에서 사용 (인스펙터에서 표 형태로 편집)
[CreateAssetMenu(fileName = "DefenceInfo", menuName = "DefenceInfo")]
public class DefenceInfo : ScriptableObject
{
    [Serializable]
    public class Row
    {
        public DefenceType defenceType;
        public float[] multipliers; // AttackType 순서 (카오스 Chaos, 물리 Physic, 마법 Magic, 진동 Vibration). 1 = 100%
    }

    public Row[] rows;

    static DefenceInfo instance;
    public static DefenceInfo Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<DefenceInfo>("DefenceInfo");
            return instance;
        }
    }

    // 에셋이 없으면 기본값 사용
    // (2026-11-22) 테스트(미니언 피해 측정) 전용: 켜면 모든 상성 배율 = 1. 기본 false
    public static bool ignoreForTest;

    public static float Get(AttackType attackType, DefenceType defenceType)
    {
        if (ignoreForTest)
            return 1f;
        DefenceInfo table = Instance;
        if (table == null)
            return GetDefault(attackType, defenceType);
        return table.GetMultiplier(attackType, defenceType);
    }

    public float GetMultiplier(AttackType attackType, DefenceType defenceType)
    {
        if (rows != null)
        {
            foreach (Row row in rows)
            {
                if (row.defenceType != defenceType)
                    continue;
                int idx = (int)attackType;
                if (row.multipliers != null && idx < row.multipliers.Length)
                    return row.multipliers[idx];
                break;
            }
        }
        return GetDefault(attackType, defenceType);
    }

    // 기획 기본값 (에셋 Reset 시, 에셋이 없을 때, 새 타입이 추가돼 칸이 생길 때 사용). 실제 값은 Resources/DefenceInfo 에셋에서 수정
    // (2026-11-21 임시)     카오스  물리  마법  진동
    //   경갑(Light)          1    1.5   1     0.75
    //   중갑(Normal)         1    1     0.75  1.5
    //   중장갑(Armored)      1    0.5   1.5   1
    //   영체(Ethereal)       1    1.25  0.5   1.25
    public static float GetDefault(AttackType attackType, DefenceType defenceType)
    {
        switch (defenceType)
        {
            case DefenceType.Light:
                return Pick(attackType, 1f, 1.5f, 1f, 0.75f);
            case DefenceType.Normal:
                return Pick(attackType, 1f, 1f, 0.75f, 1.5f);
            case DefenceType.Armored:
                return Pick(attackType, 1f, 0.5f, 1.5f, 1f);
            case DefenceType.Ethereal:
                return Pick(attackType, 1f, 1.25f, 0.5f, 1.25f);
            default:
                return 1f;
        }
    }

    static float Pick(AttackType attackType, float chaos, float physic, float magic, float vibration)
    {
        switch (attackType)
        {
            case AttackType.Physic: return physic;
            case AttackType.Magic: return magic;
            case AttackType.Vibration: return vibration;
            default: return chaos;
        }
    }

    void Reset()
    {
        rows = null;
        OnValidate();
    }

    // DefenceType/AttackType이 추가되면 행/열을 자동으로 맞춤 (기존 값 유지)
    void OnValidate()
    {
        DefenceType[] defenceTypes = (DefenceType[])Enum.GetValues(typeof(DefenceType));
        AttackType[] attackTypes = (AttackType[])Enum.GetValues(typeof(AttackType));

        Row[] newRows = new Row[defenceTypes.Length];
        for (int d = 0; d < defenceTypes.Length; d++)
        {
            Row old = rows != null ? Array.Find(rows, r => r != null && r.defenceType == defenceTypes[d]) : null;
            Row row = new Row { defenceType = defenceTypes[d], multipliers = new float[attackTypes.Length] };
            for (int a = 0; a < attackTypes.Length; a++)
            {
                bool hasOld = old != null && old.multipliers != null && a < old.multipliers.Length;
                row.multipliers[a] = hasOld ? old.multipliers[a] : GetDefault(attackTypes[a], defenceTypes[d]);
            }
            newRows[d] = row;
        }
        rows = newRows;
    }


    public static string Title(DefenceType defenceType)
    {
        // 번역 key = DefenceType.{코드 이름} (Normal = 중갑, Armored = 중장갑, Ethereal = 영체, Light = 경갑)
        return TranslateManager.GetText($"DefenceType.{defenceType}");
    }

    // 배율 표 값으로 생성 → 표를 고치면 설명도 따라감
    // 약점: 피해를 더 받는 공격 타입(배율 > 1), 내성: 덜 받는 공격 타입(배율 < 1). 1배는 생략
    public static string Desc(DefenceType defenceType)
    {
        List<string> weak = new List<string>();
        List<string> resist = new List<string>();
        foreach (AttackType attackType in Enum.GetValues(typeof(AttackType)))
        {
            float mul = Get(attackType, defenceType);
            if (mul > 1f)
                weak.Add(AttackInfo.Title(attackType));
            else if (mul < 1f)
                resist.Add(AttackInfo.Title(attackType));
        }

        if (weak.Count == 0 && resist.Count == 0)
            return TranslateManager.GetText("Defence_None");

        List<string> lines = new List<string>();
        if (weak.Count > 0)
            lines.Add(string.Format(TranslateManager.GetText("Defence_Weak"), string.Join(", ", weak)));
        if (resist.Count > 0)
            lines.Add(string.Format(TranslateManager.GetText("Defence_Resist"), string.Join(", ", resist)));
        return string.Join("\n", lines);
    }

    public static Sprite GetSprite(DefenceType dType)
    {
        return Resources.Load<Sprite>($"Icons/DefenceType/{dType}");
    }
}
