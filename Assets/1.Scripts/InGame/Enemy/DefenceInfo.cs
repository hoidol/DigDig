using System;
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
        public float[] multipliers; // AttackType 순서 (Physic, Magic, Vibrational)
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
        switch (defenceType)
        {
            case DefenceType.Armored: // 갑옷형 - 물리 -50% 마법 +100% 진동 0%
                return attackType == AttackType.Physic ? 0.5f : attackType == AttackType.Magic ? 2f : 1f;
            case DefenceType.Ethereal: // 정령형 - 물리 +100% 마법 -50% 진동 0%
                return attackType == AttackType.Physic ? 2f : attackType == AttackType.Magic ? 0.5f : 1f;
            case DefenceType.HeavyArmor: // 중갑형 - 진동 +60% 물리 -50% 마법 -50%
                return attackType == AttackType.Vibrational ? 2f : 0.5f;
            case DefenceType.Organic: // 생체형 - 물리 +50% 마법 -50% 진동 0%
                return attackType == AttackType.Physic ? 1.5f : attackType == AttackType.Magic ? 0.5f : 1f;
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
            case DefenceType.HeavyArmor:
             return "중갑형";
            case DefenceType.Organic:
             return "생체형";
        }
        return "일반형";
    }

    public static string Desc(DefenceType defenceType)
    {
        
        switch (defenceType)
        {
            case DefenceType.Normal:
             return "모든 공격 타입 +100%";
            case DefenceType.Armored:
             return "물리 <color=#FF0000>-50%</color>\n마법 <color=#00FF00>+200%</color>";
            case DefenceType.Ethereal:
             return "물리 <color=#00FF00>+200%</color>\n마법 <color=#FF0000>-50%</color>";
            case DefenceType.HeavyArmor:
             return "물리,마법 <color=#00FF00>-50%</color>\n진동 <color=#FF0000>+200%</color>";
            case DefenceType.Organic:
             return "물리 <color=#00FF00>+150%</color>\n마법 <color=#FF0000>-50%</color>";
        }
        return "물리 <color=#FF0000>-50%</color>\n마법 <color=#00FF00>+100%</color>";
    }
    public static Sprite GetSprite(DefenceType dType)
    {
        return Resources.Load<Sprite>($"Icons/DefenceType/{dType}");
    }
}
