using UnityEngine;
[CreateAssetMenu(fileName = "StatData", menuName = "Data/StatData", order = 0)]
public class StatData : ScriptableObject
{
    public StatType statType;
    public string desc;
    public Sprite thum;
    //public Stat statPrefab;

    //public Grade grade;
    [Header("-1이면 레벨업 제한없음, 또는 5")]
    public int maxLv = -1;
    public ConditionData[] conditions;
    public float initValue;
    public float increasedValue;
    public StatOpType opType;   // 어떤 방식으로 적용할지


    public virtual bool Unlocked()
    {
        foreach (var condition in conditions)
        {
            if (!condition.Check())
                return false;
        }
        return true;
    }
    public static StatData GetStatData(string key)
    {
        return StatManager.Instance.GetStatData(key);
    }
    public string GetDescription(int lv = -1)
    {
        if (statType == StatType.AttackPower)
        {
            return $"공격력 {(int)(increasedValue * lv * 100)}% 증가";
        }
        else if (statType == StatType.AngerTime)
        {
            return $"분노 시간 증가";
        }
        else if (statType == StatType.AttackRange)
        {
            return $"공격 범위 증가";
        }
        else if (statType == StatType.AttackSpeed)
        {
            return $"공격 속도 {(int)(increasedValue * lv * 100)}% 증가";
        }
        else if (statType == StatType.BulletCount)
        {
            return $"총알 개수 증가";
        }
        else if (statType == StatType.CritChance)
        {
            return $"크리티컬 확률 {(int)(increasedValue * lv * 100)}% 증가";
        }
        else if (statType == StatType.CritPower)
        {
            return $"크리티컬 파워 {(increasedValue * lv):F1}배 증가";
        }
        else if (statType == StatType.Lucky)
        {
            return $"행운 증가";
        }
        else if (statType == StatType.MagicPower)
        {
            return $"마력 +{(int)(increasedValue * lv)} 증가";
        }
        else if (statType == StatType.MoveSpeed)
        {
            return $"이동 속도 {(int)(increasedValue * lv * 100)}% 증가";
        }
        else if (statType == StatType.MaxHp)
        {
            return $"최대 체력 {(int)(increasedValue * lv * 100)}% 증가";
        }
        else if (statType == StatType.PickUpRange)
        {
            return $"픽업 범위 증가";
        }
        else if (statType == StatType.RecoveryHp)
        {
            return $"회복력 증가";
        }
        else if (statType == StatType.ReloadSpeed)
        {
            return $"재장전 속도 {(int)(increasedValue * lv * 100)}% 증가";
        }
        else if (statType == StatType.ReloadTime)
        {
            return $"재장전 시간 감속";
        }


        return $"-";
    }
    public float GetValue(int count)
    {
        return initValue + increasedValue * count;
    }

    public float Apply(float baseValue, int count)
    {
        float v = GetValue(count);
        switch (opType)
        {
            case StatOpType.Add:
                return baseValue + v;
            case StatOpType.Subtract:
                return baseValue - v;
            case StatOpType.Multiply:
                return baseValue * v;
            case StatOpType.Divide:
                return v != 0 ? baseValue / v : baseValue;
            default:
                return baseValue;
        }
    }
}
public class Stat
{
    public StatType statType;
    public int count;
    public Stat(StatType type)
    {
        statType = type;
        count = 0;
    }

    public void LevelUp()
    {
        count++;
    }

}
