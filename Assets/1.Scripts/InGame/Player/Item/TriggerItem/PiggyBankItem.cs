public class PiggyBankItem : TriggerItem
{
    public override void OnUnequip(Player player)
    {
        base.OnUnequip(player);
    }
    public override void OnTrigger()
    {
        int interest = (Player.Instance.gold / 50) * 2 * count;
        if (interest > 0)
            Player.Instance.AddGold(interest);
    }

}
