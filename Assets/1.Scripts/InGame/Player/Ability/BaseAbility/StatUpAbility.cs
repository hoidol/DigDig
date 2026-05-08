using UnityEngine;

public class StatUpAbility : Ability
{
    public StatType statType;
    public float initValue;
    public float increasedValue;

    public StatOpType opType;   // 어떤 방식으로 적용할지

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (statType == StatType.AttackPower)
        {
            if (detail) return $"공격력 {GetValue(c - 1)} > {GetValue(c)} 증가";
            else return $"공격력 증가";
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
            return $"공격 속도 증가";
        }
        else if (statType == StatType.BulletCount)
        {
            return $"총알 개수 증가";
        }
        else if (statType == StatType.CritChance)
        {
            return $"크리티컬 확률 증가";
        }
        else if (statType == StatType.CritPower)
        {
            return $"크리티컬 파워 증가";
        }
        else if (statType == StatType.Lucky)
        {
            return $"행운 증가";
        }
        else if (statType == StatType.MagicPower)
        {
            return $"마력 +1 증가";
        }
        else if (statType == StatType.MoveSpeed)
        {
            return $"이동 속도 증가";
        }
        else if (statType == StatType.MaxHp)
        {
            return $"최대 체력 증가";
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
            return $"재장전 속도 증가";
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
