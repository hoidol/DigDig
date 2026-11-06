using UnityEngine;

// 공용 스탯 강화 능력: 강화 레벨(level) 도달 시 buffs를 슬라임에 적용
// 프리팹에 붙이고 인스펙터에서 level / buffs(스탯, 값, 연산)만 설정. 슬라임 고유 능력만 별도 클래스로 만든다
// ex) 공격력 10% 증가 = AttackPower, 1.1, Multiply / 공격력 +2 = AttackPower, 2, Add
public class StatBuffEnhanceAbility : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public Buff[] buffs = new Buff[0];

    public void Force(Slime slime)
    {
        // 인스펙터 값을 그대로 넘기지 않고 복사 (버프 객체를 슬라임마다 따로 가짐)
        foreach (Buff buff in buffs)
            slime.AddBuff(new Buff(buff.statType, buff.value, buff.opType));
    }
}
