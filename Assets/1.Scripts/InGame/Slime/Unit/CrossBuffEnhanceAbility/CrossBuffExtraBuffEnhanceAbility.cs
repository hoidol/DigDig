// 십자 버프 슬라임 공용: 버프 받는 슬라임에게 추가 버프
// ex) Breeze 11: 공격력 5% 증가 (AttackPower, 1.05, Multiply) / Rage 11: 공격속도 5% 증가 (AttackSpeed, 1.05, Multiply)
public class CrossBuffExtraBuffEnhanceAbility : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public Buff[] buffs = new Buff[0];
    public void Force(Slime slime)
    {
        CrossBuffSlime cross = slime as CrossBuffSlime;
        foreach (Buff buff in buffs)
            cross.extraBuffs.Add(buff); // 대상마다 복사해서 걸림 (CrossBuffSlime.RefreshBuffs)
    }
}
