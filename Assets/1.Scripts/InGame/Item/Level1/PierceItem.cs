using UnityEngine;

//탄 4번 쏠때마다 관통탄 추가 발사 (랜덤 방향)튕김 횟수 +2 > +4 > +6
public class PierceItem : Item, IPreFire
{
    int[] pierceCounts = { 3, 5, 7 };
    int[] triggerCounts = { 5, 5, 5 };
    int triggerCounter;
    public void OnPreFire(ref Bullet bullet, Vector2 dir)
    {
        triggerCounter++;
        if (triggerCounts[count - 1] <= triggerCounter)
        {
            if (bullet == null || bullet.bulletData.order < BulletData.GetBulletData("Pierce").order)
            {
                PierceBullet pierceBullet = new PierceBullet();
                pierceBullet.pierceCount = pierceCounts[count - 1];
                bullet = pierceBullet;
            }
            triggerCounter = 0;
        }
    }

    public override string GetDescription(int lv, bool detail = false)
    {
        return $"{triggerCounts[lv - 1]}발사마다 관통탄 발사\n관통력 +{pierceCounts[lv - 1]}";
    }
}