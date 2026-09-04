using UnityEngine;

public class CloverItem : Item
{
    float chance = 0.15f;
    public override  void OnEquip()
    {
        Character.Instance.coinChance += chance;
    }
    public override void OnUnequip()
    {
        Character.Instance.coinChance -= chance;
    }

    public override string GetDescription()
    {
        return $"코인 드랍 확률 +{chance * 100:0}%";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),critChance);
    }
    
}