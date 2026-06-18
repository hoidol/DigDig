using DG.Tweening;
using UnityEngine;

public class NormalEnemy : Enemy, IHpUI
{
    public float MaxHp => maxHp;
    public float CurHp => curHp;
    public Vector3 HpUIPosition => hpPoint.position;

    HpUI hpUI;

    public override void UpdateAttack()
    {

        Vector2 vec = Player.Instance.transform.position - transform.position;
        SetFacing(vec.x);
        rg2d.linearVelocity = Vector2.zero;

        if (attackTimer >= enemyData.attackSpeed)
            StartAttack();
    }


    protected override void OnHpChanged()
    {
        if (hpUI == null || !hpUI.IsOwn(this))
            hpUI = HpUI.Get(this);
        hpUI.UpdateTime();
    }

    public override void OnDead()
    {
        base.OnDead();

        hpUI?.Release();
        Exp.Instantiate(transform.position, enemyData.exp, Size.x);

    }
}
