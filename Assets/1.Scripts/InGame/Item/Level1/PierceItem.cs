using UnityEngine;

// 탄 관통력 추가
public class PierceItem : Item, IFired
{
    int pierceCount = 2;

    public void OnFired(ref BulletSpec bullet, ref AllyBulletObject bulletObject, Vector2 dir)
    {
        bulletObject.AddBehavior(new PierceBehavior(Character.Instance.itemInventory.GetItem(key).count * pierceCount));        
    }
    public override string GetDescription()
    {
        return $"관통력 +{pierceCount}";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),triggerCount,pierceCount);
    }
}