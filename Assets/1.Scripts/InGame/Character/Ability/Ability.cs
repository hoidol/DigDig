using UnityEngine;

public abstract class Ability : CharacterEnhancement
{
    public AbilityData abilityData;// => AbilityManager.Instance.GetAbilityData(key);

    public override void OnEquip(Character player) { }
    public override void OnUnequip(Character player) { }
}
