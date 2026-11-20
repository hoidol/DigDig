using UnityEngine;

// 킹 능력치 증가 (공격력·공속·사거리·체력·치명타). 레벨마다 valuePerLevel씩 늘어난 Buff 하나를 킹에 건다
// Multiply: 1 + valuePerLevel × level (0.2 → Lv2 = ×1.4) / Add: valuePerLevel × level
// 최대 체력이 오르면 오른 만큼 회복 (KingSlime.UpdateCharacter)
public class StatKingAbility : KingAbility
{
    public StatType statType = StatType.AttackPower;
    public StatOpType opType = StatOpType.Multiply;
    public float valuePerLevel = 0.2f;

    Buff buff;

    public float GetValue(int level) => opType == StatOpType.Multiply ? 1f + valuePerLevel * level : valuePerLevel * level;

    // Multiply는 % 증가량 (Lv2, 0.2 → "40"), Add는 값 그대로
    public override string GetValueText(int level) =>
        opType == StatOpType.Multiply ? FormatNumber(valuePerLevel * level * 100f) : FormatNumber(valuePerLevel * level);

    public override void OnEquip() => ApplyBuff();
    public override void OnLevelChanged() => ApplyBuff();
    public override void OnUnequip() => RemoveBuff();

    void ApplyBuff()
    {
        RemoveBuff();
        buff = new Buff(statType, GetValue(level), opType);
        king.AddBuff(buff);
    }

    void RemoveBuff()
    {
        if (buff == null || king == null)
            return;
        king.RemoveBuff(buff);
        buff = null;
    }
}
