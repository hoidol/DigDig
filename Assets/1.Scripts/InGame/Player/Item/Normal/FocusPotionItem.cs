// 집중력 물약: 공격력 +2 (소모성, 중첩 가능)
public class FocusPotionItem : Item
{
    const float BONUS_PER_STACK = 3f;
    Buff buff;

    public override void OnEquip(Player player)
    {
        count++;
        UpdateItem();
    }

    public override void UpdateItem()
    {
        Player player = Player.Instance;

        if (buff != null) player.RemoveBuff(buff);

        buff = new Buff(StatType.AttackPower, BONUS_PER_STACK * count, StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(bool detail = false)
        => $"공격력 +{BONUS_PER_STACK * count}";
}
