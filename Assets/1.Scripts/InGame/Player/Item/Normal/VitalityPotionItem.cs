// 체력 물약: 최대 체력 +10 (소모성, 중첩 가능)
public class VitalityPotionItem : Item
{
    const float BONUS_PER_STACK = 20f;
    Buff buff;

    public override void OnEquip(Player player)
    {
        UpdateItem();
    }

    public override void UpdateItem()
    {
        Player player = Player.Instance;

        if (buff != null) player.RemoveBuff(buff);

        buff = new Buff(StatType.MaxHp, BONUS_PER_STACK * GetLevel(), StatOpType.Add);
        player.AddBuff(buff);
    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
    }

    public override string GetDescription(bool detail = false)
        => $"체력 +{BONUS_PER_STACK * GetLevel()}";
}
