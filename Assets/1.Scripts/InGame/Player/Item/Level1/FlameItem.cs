using UnityEngine;

public class FlameItem : Item, IFired
{
    
    public int[] burnDurations = {5,6,7};
    public int[] burnDPSs = {2,3,4};
    public int[] triggerCounts = {4,4,4};

    int triggerCounter;
    public override void OnEquip()
    {
        base.OnEquip();
        triggerCounter= 0;
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"{triggerCounts[lv-1] * 100:0} 공격마다 화염탄 랜덤 방향으로 발사";
    }

    public void OnFired(Vector2 dir)
    {
          triggerCounter++;
        if (triggerCounts[count-1] <=triggerCounter)
        {
            FlameBullet flameBullet = new FlameBullet();
            flameBullet.burnDuration  = burnDurations[count -1];
            flameBullet.burnDPS  = burnDPSs[count -1];

            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Player.Instance.Shoot(flameBullet, randomDir, Player.Instance.attackPoint.position);

            triggerCounter=0;
        }
    }
}