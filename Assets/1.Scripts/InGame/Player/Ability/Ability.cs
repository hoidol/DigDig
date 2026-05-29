using UnityEngine;

public abstract class Ability : PlayerEnhancement
{
    public AbilityData abilityData => AbilityManager.Instance.GetAbilityData(key);



    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }
}
