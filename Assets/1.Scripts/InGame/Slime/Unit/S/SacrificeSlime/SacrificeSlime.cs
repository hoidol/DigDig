using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

// [S] 희생: 같은 머지 레벨의 아무 슬라임과 합성 가능 → 결과 = 상대 슬라임과 같은 종(key), 머지 레벨 +1
// - 일반 합성은 결과 종이 랜덤이지만, 희생은 키우고 싶은 종을 골라 올릴 수 있음
// - 끌어온 쪽이든 놓인 쪽이든 적용 (SlimeSpawner.Merge). 희생끼리 합성은 일반 규칙(랜덤 종)
// - 강화: 5/11 = 합성 결과 슬라임 버프(ResultBuffEnhanceAbility), 8 = 50% 확률로 머지 레벨 +1 추가
// 평소엔 일반 탄 공격
public class SacrificeSlime : SlimeGrowth1, IResultBuffSlime
{
    SacrificeSlimeData sacrificeSlimeData;
    AllyBulletSpec allyBulletSpec;

    // 강화 능력이 바꿀 수 있음 (InitSlime에서 base 호출 전에 기본값 설정)
    public float extraLevelChance;           // 0~1, 합성 시 머지 레벨 +1 추가 확률 ([각성])
    readonly List<Buff> resultBuffs = new List<Buff>();
    public List<Buff> ResultBuffs => resultBuffs;

    public override void Spawn(Vector2 pos, int lv)
    {
        sacrificeSlimeData = SlimeManager.Instance.GetSlimeData(key) as SacrificeSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        allyBulletSpec = new AllyBulletSpec();
        extraLevelChance = 0f;
        resultBuffs.Clear();
        base.InitSlime();
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
        if (lv >= SlimeData.MAX_MERGE_LEVEL)
            return false;
        return true;
    }

    public override async UniTask<(bool, string, int)> Merge(Slime target)
    {
        if (target is SacrificeSlime)
            return await base.Merge(target);

        if (!CanSacrificeMerge(mergeLevel, SlimeData.growth, target.mergeLevel, target.SlimeData.growth))
            return (false, null, 0);

        int lv = mergeLevel + 1;
        if (extraLevelChance > 0f && Random.value < extraLevelChance)
            lv++;
        return (true, target.key, Mathf.Min(lv, SlimeData.MAX_MERGE_LEVEL));
    }

    public void ApplyResultBuffs(Slime result) => ResultBuffEnhanceAbility.Apply(this, result);
}
