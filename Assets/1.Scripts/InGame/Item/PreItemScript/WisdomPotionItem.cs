// 정신력 물약: 마력 +1 (소모성, 중첩 가능)
public class WisdomPotionItem : Item
{
    const float BONUS_PER_STACK = 1f;
    Buff buff;

    public override void UpdateItem()
    {
        Character player = Character.Instance;

        if (buff != null) player.RemoveBuff(buff);

        // buff = new Buff(StatType.MagicPower, BONUS_PER_STACK * count, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void OnUnequip()
    {
        Character.Instance.RemoveBuff(buff);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
        => $"마력 +{BONUS_PER_STACK * count}";
}
