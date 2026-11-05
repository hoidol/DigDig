using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    public AbilityData abilityData;// => AbilityManager.Instance.GetAbilityData(key);

    public virtual string GetDescription(bool detail = false){return null;}
     public virtual void OnEquip(KingSlime player) { }
    public virtual void OnUnequip(KingSlime player) { }
}
