using UnityEngine;

// [B] 낙인: 일반 탄 + 적중 시 확률로 낙인 (StigmaEffect). 낙인 중첩당 그 적이 받는 모든 피해 증가
// 확률은 머지 레벨(StigmaSlimeData.stigmaChances), 중첩당 피해/최대 중첩은 StigmaSlimeData
// [각성] (진화) 낙인 최대 중첩 EVOLVE_MAX_STACK
public class StigmaSlime : SlimeGrowth1
{
    public const int EVOLVE_MAX_STACK = 5;
    StigmaSlimeData stigmaSlimeData;
    AllyBulletSpec allyBulletSpec;

    public float stigmaChance;
    public int maxStack;
    public float damagePerStack;

    public override void Spawn(Vector2 pos, int lv)
    {
        stigmaSlimeData = SlimeManager.Instance.GetSlimeData(key) as StigmaSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        allyBulletSpec = new AllyBulletSpec();
        stigmaChance = stigmaSlimeData.stigmaChances.GetByMergeLevel(mergeLevel);
        maxStack = stigmaSlimeData.maxStack;
        damagePerStack = stigmaSlimeData.damagePerStack;
        base.InitSlime();
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        allyBulletSpec.AddBulletBehaviour(new StigmaOnHitBehavior(stigmaChance, isEvolved ? Mathf.Max(maxStack, EVOLVE_MAX_STACK) : maxStack, damagePerStack));
        return allyBulletSpec.Instantiate(this);
    }
}
