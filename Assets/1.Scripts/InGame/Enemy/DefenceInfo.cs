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
        public float[] multipliers; // AttackType 순서 (AllRounder, Physic, Magic). AllRounder는 표와 상관없이 항상 1
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
    public static float Get(AttackType attackType, DefenceType defenceType)
    {
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

    // 기획 기본값 (에셋 Reset 시, 에셋이 없을 때 사용)
    public static float GetDefault(AttackType attackType, DefenceType defenceType)
    {
        if (attackType == AttackType.AllRounder) // 만능: 모든 방어 타입에 100%
            return 1f;
        switch (defenceType)
        {
            case DefenceType.Armored: // 갑옷형 - 물리 -80% 마법 +100%
                return attackType == AttackType.Physic ? 0.2f : attackType == AttackType.Magic ? 2.0f : 1f;
            case DefenceType.Ethereal: // 정령형 - 물리 +100% 마법 -80%
                return attackType == AttackType.Physic ? 2.0f : attackType == AttackType.Magic ? 0.2f : 1f;
            case DefenceType.Organic: // 생체형 - 물리 +50% 마법 -50%
                return attackType == AttackType.Physic ? 1.5f : 0.5f;
            default: // 일반형
                return 1f;
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
        switch (defenceType)
        {
            case DefenceType.Normal:
             return "일반형";
            case DefenceType.Armored:
             return "갑옷형";
            case DefenceType.Ethereal:
             return "정령형";
            case DefenceType.Organic:
             return "생체형";
        }
        return "일반형";
    }

    // 배율 표 값으로 생성 → 표를 고치면 설명도 따라감
    // 약점: 피해를 더 받는 공격 타입(배율 > 1), 내성: 덜 받는 공격 타입(배율 < 1). 1배는 생략
    public static string Desc(DefenceType defenceType)
    {
        List<string> weak = new List<string>();
        List<string> resist = new List<string>();
        foreach (AttackType attackType in Enum.GetValues(typeof(AttackType)))
        {
            if (attackType == AttackType.AllRounder)
                continue;
            float mul = Get(attackType, defenceType);
            if (mul > 1f)
                weak.Add(AttackInfo.Title(attackType));
            else if (mul < 1f)
                resist.Add(AttackInfo.Title(attackType));
        }

        if (weak.Count == 0 && resist.Count == 0)
            return "약점/내성 없음";

        List<string> lines = new List<string>();
        if (weak.Count > 0)
            lines.Add($"약점:<color=#00FF00>{string.Join(", ", weak)}</color>");
        if (resist.Count > 0)
            lines.Add($"내성:<color=#FF0000>{string.Join(", ", resist)}</color>");
        return string.Join("\n", lines);
    }

    public static Sprite GetSprite(DefenceType dType)
    {
        return Resources.Load<Sprite>($"Icons/DefenceType/{dType}");
    }
}
