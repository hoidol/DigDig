using UnityEngine;

public class FlameItem : Item, IFired
{
    //2초마다 랜덤 방향으로 Flame 발사
    int burnDuration = 4;
    int burnDPS = 2;
    int triggerCount = 3;



    int triggerCounter;
    public override void OnEquip()
    {
        base.OnEquip();
        triggerCounter = 0;
    }

    public override string GetDescription()
    {
        return $"{triggerCount}공격마다 화염탄 2발 랜덤 방향으로 발사";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),triggerCount);
    }

    public void OnFired(ref Bullet bullet, ref CharacterBulletObject bulletObject, Vector2 dir)
    {
        triggerCounter++;
        if (triggerCount <= triggerCounter)
        {
            for(int i = 0;i < count; i++)
            {
                FlameBullet flameBullet = new FlameBullet();
                flameBullet.burnDuration = burnDuration;
                flameBullet.burnDPS = burnDPS;

                Vector2 randomDir = Random.insideUnitCircle.normalized;
                Character.Instance.Shoot(flameBullet, randomDir);

                randomDir = Random.insideUnitCircle.normalized;
                Character.Instance.Shoot(flameBullet, randomDir);

                // Character.Instance.AddHp(-itemData.consumeHp);
            }
            

            triggerCounter = 0;
        }
    }
}