using UnityEngine;

// [B] 낙인: 일반 탄 + 적중 시 확률로 낙인 (StigmaEffect). 낙인 중첩당 그 적이 받는 모든 피해 증가
// 확률은 머지 레벨(StigmaMinionData.stigmaChances), 중첩당 피해/최대 중첩은 StigmaMinionData
// [각성] (진화) 낙인 최대 중첩 EVOLVE_MAX_STACK
public class StigmaMinion : MinionGrowth1
{
    public const int EVOLVE_MAX_STACK = 5;
    StigmaMinionData stigmaMinionData;
    AllyBulletSpec allyBulletSpec;

    public float stigmaChance;
    public int maxStack;
    public float damagePerStack;

    public override void Spawn(Vector2 pos, int lv)
    {
        stigmaMinionData = MinionManager.Instance.GetMinionData(key) as StigmaMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        stigmaChance = stigmaMinionData.stigmaChances.GetByMergeLevel(mergeLevel);
        maxStack = stigmaMinionData.maxStack;
        damagePerStack = stigmaMinionData.damagePerStack;
        base.InitMinion();
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        allyBulletSpec.AddBulletBehaviour(new StigmaOnHitBehavior(stigmaChance, isEvolved ? Mathf.Max(maxStack, EVOLVE_MAX_STACK) : maxStack, damagePerStack));
        return allyBulletSpec.Instantiate(this);
    }
}
