using UnityEngine;

public class BounceItem : Item, IFired
{
    int bounceCount = 2;
    public void OnFired(ref BulletSpec bullet, ref AllyBulletObject bulletObject, Vector2 dir)
    {
        bulletObject.AddBehavior(new BounceBehavior(count * bounceCount));
    }

    public override string GetDescription()
    {
        return $"탄 튕김 +{bounceCount}";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),bounceCount);
    }
}