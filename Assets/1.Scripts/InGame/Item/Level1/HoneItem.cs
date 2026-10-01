using UnityEngine;

// 탄 관통력 추가
public class HoneItem : Item, IFired
{
    int pierceCount = 1;
    float multi =0.7f;
    public void OnFired(ref BulletSpec bullet, ref AllyBulletObject bulletObject, Vector2 dir)
    {
        bulletObject.AddBehavior(new PierceBehavior(1+count * pierceCount,multi));        
    }
    public override string GetDescription()
    {
        return $"관통력 +{pierceCount}";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),triggerCount,pierceCount);
    }
}