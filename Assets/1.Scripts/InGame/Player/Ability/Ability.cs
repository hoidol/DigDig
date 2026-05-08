using UnityEngine;

public abstract class Ability : PlayerEnhancement
{
    public AbilityData abilityData => AbilityManager.Instance.GetAbilityData(key);
    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return "스킬을 설명합니다";
    }


    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }
}
