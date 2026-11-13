using UnityEngine;

// [S] 사냥꾼: 평소엔 일반 탄, snipeInterval(3)회 공격마다 저격탄
// 저격탄 = 관통(감쇠 없음) + 맞은 적 스턴(엘리트/보스는 절반) + 확정 치명타
// - 한 번의 공격에 탄이 여러 발(다중 타겟 타일 등)이면 그 공격의 탄 전부가 저격탄
// - 연발 타일은 같은 spec을 다시 쓰므로 저격탄의 연발도 저격탄
// 프리팹 탄: 저격탄은 관통 탄(Pierce) 프리팹을 같이 씀 (전용 연출이 필요하면 별도 key로 분리)
// [각성] (진화) EVOLVE_SNIPE_INTERVAL회 공격마다 저격탄
public class HunterMinion : MinionGrowth1
{
    public const int EVOLVE_SNIPE_INTERVAL = 2;
    HunterMinionData hunterMinionData;
    AllyBulletSpec normalSpec;
    PierceBulletSpec snipeSpec;

    public int snipeInterval;
    public float snipeStunDuration;

    int attackCount;
    bool isSnipeTurn;

    public override void Spawn(Vector2 pos, int lv)
    {
        hunterMinionData = MinionManager.Instance.GetMinionData(key) as HunterMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        normalSpec = new AllyBulletSpec();
        snipeSpec = new PierceBulletSpec();
        snipeInterval = hunterMinionData.snipeInterval;
        snipeStunDuration = hunterMinionData.snipeStunDuration;
        attackCount = 0;
        base.InitMinion();
    }

    protected override void OnEvolve()
    {
        snipeInterval = Mathf.Min(snipeInterval, EVOLVE_SNIPE_INTERVAL);
    }

    public override void Fire(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return;

        isSnipeTurn = (attackCount + 1) % Mathf.Max(1, snipeInterval) == 0;
        base.Fire(dir);
        if (attackTimer == 0) // 발사에 성공했을 때만 횟수 증가
            attackCount++;
        isSnipeTurn = false;
    }

    public override AllyBulletObject GetBullet()
    {
        if (!isSnipeTurn)
        {
            normalSpec.damage = AttackPower;
            normalSpec.mustCrit = false;
            normalSpec.StartBulletSpec();
            return normalSpec.Instantiate(this);
        }

        snipeSpec.damage = AttackPower;
        snipeSpec.pierceCount = hunterMinionData.snipePierceCount;
        snipeSpec.multi = 1f; // 관통해도 피해 감소 없음
        snipeSpec.mustCrit = true;
        snipeSpec.StartBulletSpec();
        snipeSpec.AddBulletBehaviour(new StunOnHitBehavior(snipeStunDuration));
        return snipeSpec.Instantiate(this);
    }
}
