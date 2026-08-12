using UnityEngine;

// 탄 관통력 추가
public class PierceItem : Item, IFired
{
    int pierceCount = 2;
    int triggerCount = 5;
    int triggerCounter;

    public void OnFired(ref Bullet bullet, ref CharacterBulletObject playerBulletObject, Vector2 dir)
    {
        if (triggerCount <= triggerCounter)
        {
            playerBulletObject.AddBehavior(new PierceBehavior(Character.Instance.itemInventory.GetItem(key).count * pierceCount));
            triggerCounter = 0;
        }
        
    }
    public override string GetDescription()
    {
        return $"{triggerCount}발사마다 관통탄 발사\n관통력 +{pierceCount}";
    }
}