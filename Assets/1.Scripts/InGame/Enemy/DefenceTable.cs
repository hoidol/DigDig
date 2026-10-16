using System;
using UnityEngine;

// 공격 속성(AttackType) × 방어 타입(DefenceType) 피해 배율 표
// Resources/DefenceTable 에셋 하나를 게임 전역에서 사용 (인스펙터에서 표 형태로 편집)
[CreateAssetMenu(fileName = "DefenceTable", menuName = "DefenceTable")]
public class DefenceTable : ScriptableObject
{
    [Serializable]
    public class Row
    {
        public DefenceType defenceType;
        public float[] multipliers; // AttackType 순서 (Physic, Magic, Vibrational)
    }

    public Row[] rows;

    static DefenceTable instance;
    public static DefenceTable Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<DefenceTable>("DefenceTable");
            return instance;
        }
    }

    // 에셋이 없으면 기본값 사용
    public static float Get(AttackType attackType, DefenceType defenceType)
    {
        DefenceTable table = Instance;
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
        switch (defenceType)
        {
            case DefenceType.Armored: // 갑옷형 - 물리 -50% 마법 +50% 진동 0%
                return attackType == AttackType.Physic ? 0.5f : attackType == AttackType.Magic ? 1.5f : 1f;
            case DefenceType.Ethereal: // 정령형 - 물리 +50% 마법 -50% 진동 0%
                return attackType == AttackType.Physic ? 1.5f : attackType == AttackType.Magic ? 0.5f : 1f;
            case DefenceType.HeavyArmor: // 중갑형 - 진동 +60% 물리 -30% 마법 -30%
                return attackType == AttackType.Vibrational ? 1.6f : 0.7f;
            case DefenceType.Organic: // 생체형 - 물리 +30% 마법 -30% 진동 0%
                return attackType == AttackType.Physic ? 1.3f : attackType == AttackType.Magic ? 0.7f : 1f;
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
}
