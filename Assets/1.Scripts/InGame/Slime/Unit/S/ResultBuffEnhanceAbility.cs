using System.Collections.Generic;

// S등급 공용: 이 슬라임이 "다른 슬라임을 만들어 낼 때"(희생 합성 결과, 조커 변신 결과) 그 결과 슬라임에 거는 버프
// ex) 희생 5: 합성 결과 공격력 10% (AttackPower, 1.1, Multiply) / 희생 11: 공격속도 10% / 조커 11: 변신한 슬라임 공격력 10%
// 결과 슬라임 개체에 걸리므로 그 슬라임이 다시 합성되면 사라짐
public interface IResultBuffSlime
{
    List<Buff> ResultBuffs { get; }
}

public class ResultBuffEnhanceAbility : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public Buff[] buffs = new Buff[0];

    public void Force(Slime slime)
    {
        if (slime is IResultBuffSlime resultBuffSlime)
            resultBuffSlime.ResultBuffs.AddRange(buffs);
    }

    // 결과 슬라임에 버프 적용 (개체마다 복사)
    public static void Apply(IResultBuffSlime from, Slime result)
    {
        if (from == null || result == null)
            return;
        foreach (Buff buff in from.ResultBuffs)
            result.AddBuff(new Buff(buff.statType, buff.value, buff.opType));
    }
}
