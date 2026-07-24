using UnityEngine;

//적 처치 시 7초마다 360도 발사 5발 체력-2
public class SprayItem : TriggerItem 
{
    float[] coolTimes = {7,7,7};
    int[] bulletCounts = {4,6,8};
    public override void UpdateItem()
    {
        base.UpdateItem();
        coolTime = coolTimes[count-1];
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        int bulletCount = bulletCounts[count-1];
        float angleStep = 360f / bulletCount;
        Vector2 baseDir = Random.insideUnitCircle.normalized;
        for (int i = 0; i < bulletCount; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angleStep * i) * baseDir;
            Player.Instance.Shoot(new NormalBullet(), dir);
        }

        Player.Instance.AddHp(-itemData.consumeHp);
    }


    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"{coolTimes[lv-1]}초 마다 {bulletCounts[lv-1]}탄 분사 발사";
    }
    
    
}