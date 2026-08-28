using UnityEngine;


//7초마다 속도 빨라지고 랜덤 방향으로 화염탄 발사
public class FireworkItem : TriggerCycleItem, IFired
{

    public int burnDuration =  5;
    public int burnDPS =  2;

    float baseCoolTime = 7;
    float attackSpeed = 0.6f;
    float duration = 4;


    Buff attackSpeedBuff;

    public override void UpdateItem()
    {
        base.UpdateItem();
        coolTime = baseCoolTime;
        activeTime = duration *count;
    }
    public override void OnUnequip()
    {
        base.OnUnequip();
        OnDeactivate();
    }

    public override void OnActivate()
    {
        IsActive = true;
        attackSpeedBuff = new Buff(StatType.AttackSpeed, attackSpeed, StatOpType.Multiply);
        Character.Instance.AddBuff(attackSpeedBuff);

        Character.Instance.AddHp(-itemData.consumeHp);
    }

    public override string GetDescription()
    {
        return $"{duration}동안 발사 시 화염탄 랜덤 방향으로 난 및 공격 속도 증가\n쿨타임 : {baseCoolTime}초, 발사 당 체력 -{itemData.consumeHp}";
        //return string.Format(TranslateManager.GetText($"{key}_Desc"),duration,baseCoolTime,itemData.consumeHp);
    }

    public override void OnDeactivate()
    {
        if (attackSpeedBuff != null)
            Character.Instance.RemoveBuff(attackSpeedBuff);

        attackSpeedBuff = null;
        IsActive = false;
    }

    public void OnFired(ref BulletSpec bullet, ref AllyBulletObject bulletObject, Vector2 dir)
    {
        if (!IsActive)
            return;

        FlameBulletSpec flameBullet = new FlameBulletSpec();
        flameBullet.burnDuration = burnDuration;
        flameBullet.burnDPS = burnDPS;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Character.Instance.Shoot(flameBullet, randomDir);
    }
}