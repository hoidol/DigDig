using Cysharp.Threading.Tasks;
using UnityEngine;

// [S] 희생: 같은 머지 레벨의 아무 슬라임과 합성 가능 → 결과 = 상대 슬라임과 같은 종(key), 머지 레벨 +1
// - 일반 합성은 결과 종이 랜덤이지만, 희생은 키우고 싶은 종을 골라 올릴 수 있음
// - 끌어온 쪽이든 놓인 쪽이든 적용 (MinionFieldController.Merge). 희생끼리 합성은 일반 규칙(랜덤 종)
// 평소엔 일반 탄 공격
public class SacrificeMinion : MinionGrowth1
{
    // 최대 머지 레벨에서는 쓸 수 있는 각성이 없어 진화하지 않음 (머지 레벨을 올리는 슬라임)
    public override bool CanEvolve => false;

    SacrificeMinionData sacrificeMinionData;
    AllyBulletSpec allyBulletSpec;

    public override void Spawn(Vector2 pos, int lv)
    {
        sacrificeMinionData = MinionManager.Instance.GetMinionData(key) as SacrificeMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        base.InitMinion();
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }

    // 희생 합성 조건: 종 상관없이 같은 mergeLv, 최대 머지 레벨 미만
    public static bool CanSacrificeMerge(int lv, int growth, int targetLv, int targetGrowth)
    {
        if (targetGrowth != growth)
            return false;
        if (lv != targetLv)
            return false;
        if (lv >= MinionData.MAX_MERGE_LEVEL)
            return false;
        return true;
    }

    public override async UniTask<(bool, string, int)> Merge(Minion target)
    {
        if (target is SacrificeMinion)
            return await base.Merge(target);

        if (!CanSacrificeMerge(mergeLevel, MinionData.growth, target.mergeLevel, target.MinionData.growth))
            return (false, null, 0);

        return (true, target.key, Mathf.Min(mergeLevel + 1, MinionData.MAX_MERGE_LEVEL));
    }
}
