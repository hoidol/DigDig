using UnityEngine;

public class LightningRodBulletObject : CharacterBulletObject
{
    int killCount;
     int lv;
     DamageBoostForce damageBoostForce;

    public override IHittable Hit(RaycastHit2D hit2D)
    {
        IHittable result = base.Hit(hit2D);
        if (result != null && result.Transform.TryGetComponent<LightningRodMark>(out LightningRodMark lRMrk))
        {
            
        }


        return result;
    }
}