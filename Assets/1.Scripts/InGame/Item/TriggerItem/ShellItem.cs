public class ShellItem : TriggerItem
{
    StatusEffectHandler shieldHandler;

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        shieldHandler = player.GetComponent<StatusEffectHandler>();
    }

    public override void OnTrigger()
    {
        shieldHandler?.Apply(new ShieldEffect());
    }
}
