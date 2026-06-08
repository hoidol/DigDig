// 적 처치 시 최대 체력의 healRate만큼 회복 (0.005 = 0.5%)
using UnityEngine;
public class VampireOnKillBehavior : IBulletBehavior
{
    readonly float healRate;
    public VampireOnKillBehavior(float healRate) { this.healRate = healRate; }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D)
    {
        if (hit is Enemy enemy && enemy.CurHp <= 0)
            Player.Instance.AddHp(Player.Instance.statMgr.MaxHp * healRate);
        return true;
    }

    public void OnMove(BulletObject bullet) { }
    public void Merge(IBulletBehavior other) { }
}
