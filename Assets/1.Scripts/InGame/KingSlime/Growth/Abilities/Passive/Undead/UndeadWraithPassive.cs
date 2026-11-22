using UnityEngine;

// 언데드 · 공격형 · 망령 소환: 적이 처치된 자리에서 chances 확률로 망령(킹 탄)이 튀어나와 searchRange 안 가장 가까운 적에게 날아감
// 피해 = 킹 공격력 × damageMul. 망령 탄은 킹 성장 능력(OnKingFired)을 거치지 않음 → 망령이 다시 망령을 부르는 연쇄는 확률로만
public class UndeadWraithPassive : PassiveKingAbility
{
    public float[] chances = { 0.2f, 0.3f, 0.4f };
    public float damageMul = 1f;
    public float searchRange = 4f;

    protected override bool ListenKills => true;

    public override object[] GetDescArgs(int level) => new object[] { Percent(LV(chances, level)), Percent(damageMul) };

    protected override void Apply() { }

    protected override void OnEnemyKilled(Enemy dead, Vector2 pos)
    {
        if (Random.value > LV(chances) || king.statMgr == null || EnemyFieldController.Instance == null)
            return;

        Enemy target = null;
        float best = searchRange * searchRange;
        foreach (Enemy e in EnemyFieldController.Instance.activeEnemies)
        {
            if (e == null || e == dead || e.CurHp <= 0)
                continue;
            float d = ((Vector2)e.transform.position - pos).sqrMagnitude;
            if (d <= best)
            {
                best = d;
                target = e;
            }
        }
        if (target == null)
            return;

        var bullet = new KingSlimeBulletSpec().Instantiate(king) as KingSlimeBulletObject;
        if (bullet == null)
            return;
        bullet.transform.position = pos;
        Vector2 dir = ((Vector2)target.transform.position - pos).normalized;
        bullet.Shoot(dir, king.statMgr.AttackPower * damageMul);
    }
}
