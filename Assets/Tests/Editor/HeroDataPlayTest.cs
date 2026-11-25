using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-24) 킹 기본 스탯: 용사 고르기 전 = KingSlimeData.csv, 용사를 고르면 = HeroData.csv의 그 용사 행 (+ 용사 강화 포인트)
public class HeroDataPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator HeroPick_SwapsKingBaseStats()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    static float Expected(float baseValue, StatType statType) => KingSlimeManager.Instance.GetTotalStatValue(baseValue, statType);

    IEnumerator Body()
    {
        KingSlime king = KingSlime.Instance;
        KingSlimeData ksData = king.statMgr.kingslimeData;
        Assert.IsNull(king.statMgr.heroData, "시작 = 용사 없음");
        foreach (StatType st in HeroDataTable.STAT_COLUMNS)
            Assert.AreEqual(Expected(ksData.GetKingSlimeStat(st)?.value ?? 0f, st), king.statMgr.statDic[st].initValue, 0.0001f, $"용사 전 {st} = KingSlimeData");

        // 데이터: HeroData.csv → Resources/HeroData (Knight 행)
        HeroDataTable.HeroData knight = HeroDataTable.Get("Knight");
        Assert.IsNotNull(knight, "HeroData.csv에 Knight 행 없음 (Resources/HeroData 임포트)");

        // 레벨업에 나오는 용사마다 HeroData 행이 있어야 함 (key = 클래스 이름에서 Hero를 뺀 것)
        var setting = KingGrowthSetting.Instance;
        foreach (AbilityData d in setting.heroAbilities)
            if (d.prefab is HeroKingAbility h)
                Assert.IsNotNull(HeroDataTable.Get(h.HeroDataKey), $"HeroData.csv에 {h.HeroDataKey} 행 없음 ({d.key})");

        // 기사 선택 → 킹 기본 스탯 = Knight 행
        AbilityData knightData = setting.heroAbilities.First(d => d.prefab is KnightHero);
        king.abilityInventory.Add(knightData);
        yield return WaitFrames(2);
        Assert.AreEqual("Knight", king.statMgr.heroData?.key, "기사 → HeroData Knight");
        foreach (var s in knight.stats)
            Assert.AreEqual(Expected(s.value, s.statType), king.statMgr.statDic[s.statType].initValue, 0.0001f, $"기사 {s.statType} = HeroData");

        // 값이 다른 용사 행으로 바꾸면 스탯·최대 체력이 바뀜 (체력 증가분만큼 현재 체력도)
        var custom = new HeroDataTable.HeroData { key = "TestHero" };
        custom.stats.Add(new HeroDataTable.HeroStat { statType = StatType.AttackPower, value = 99f });
        custom.stats.Add(new HeroDataTable.HeroStat { statType = StatType.MaxHp, value = 50f });
        float hp = king.health.CurHp, maxHp = king.statMgr.MaxHp;
        king.ApplyHeroBaseStats(custom);
        Assert.AreEqual(Expected(99f, StatType.AttackPower), king.statMgr.AttackPower, 0.0001f, "공격력 = 용사 행");
        Assert.AreEqual(Expected(50f, StatType.MaxHp), king.statMgr.MaxHp, 0.0001f, "최대 체력 = 용사 행");
        Assert.AreEqual(hp + (king.statMgr.MaxHp - maxHp), king.health.CurHp, 0.0001f, "늘어난 최대 체력만큼 현재 체력");
        Assert.AreEqual(Expected(ksData.GetKingSlimeStat(StatType.AttackSpeed)?.value ?? 0f, StatType.AttackSpeed), king.statMgr.statDic[StatType.AttackSpeed].initValue, 0.0001f,
            "용사 행에 없는 스탯 = KingSlimeData");

        // 되돌리기 (용사 해제)
        king.ApplyHeroBaseStats(null);
        Assert.AreEqual(Expected(ksData.GetKingSlimeStat(StatType.AttackPower)?.value ?? 0f, StatType.AttackPower), king.statMgr.AttackPower, 0.0001f, "해제 = KingSlimeData");
    }
}
