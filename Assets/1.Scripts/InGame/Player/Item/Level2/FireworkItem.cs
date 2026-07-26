using UnityEngine;


//7초마다  타이밍->없어지는게 문제야 
public class FireworkItem : TriggerCycleItem, IFired
{

    public int[] burnDurations = { 5, 6, 7 };
    public int[] burnDPSs = { 2, 3, 4 };

    float[] coolTimes = { 7, 7, 7 };
    float[] attackSpeeds = { 0.6f, 0.6f, 0.6f };
    float[] durations = { 4f, 5f, 6f };


    Buff attackSpeedBuff;

    public override void UpdateItem()
    {
        base.UpdateItem();
        coolTime = coolTimes[count - 1];
        activeTime = durations[count - 1];
    }
    public override void OnUnequip()
    {
        base.OnUnequip();
        OnDeactivate();
    }

    public override void OnActivate()
    {
        IsActive = true;
        attackSpeedBuff = new Buff(StatType.AttackSpeed, attackSpeeds[count - 1], StatOpType.Multiply);
        Player.Instance.AddBuff(attackSpeedBuff);

        Player.Instance.AddHp(-itemData.consumeHp);
    }

    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"{durations[lv - 1]}동안 발사 시 화염탄 랜덤 방향으로 난 및 공격 속도 증가\n쿨타임 : {coolTimes[lv - 1]}초, 발사 당 체력 -{itemData.consumeHp}";
    }

    public override void OnDeactivate()
    {
        if (attackSpeedBuff != null)
            Player.Instance.RemoveBuff(attackSpeedBuff);

        attackSpeedBuff = null;
        IsActive = false;
    }

    public void OnFired(ref Bullet bullet, ref PlayerBulletObject playerBulletObject, Vector2 dir)
    {
        if (!IsActive)
            return;

        FlameBullet flameBullet = new FlameBullet();
        flameBullet.burnDuration = burnDurations[count - 1];
        flameBullet.burnDPS = burnDPSs[count - 1];

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Player.Instance.Shoot(flameBullet, randomDir);
    }
}