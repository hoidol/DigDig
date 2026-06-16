// 적 처치 시 최대 체력의 healRate만큼 회복 (0.005 = 0.5%)
using UnityEngine;
public class VampireOnKillBehavior : IBulletBehavior
{
    readonly float healRate;
    public VampireOnKillBehavior(float healRate) { this.healRate = healRate; }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (hit is Enemy enemy && enemy.curHp <= 0)
            Player.Instance.AddHp(Player.Instance.statMgr.MaxHp * healRate);
        return true;
    }

}
