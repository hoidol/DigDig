using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-24) 슬라임 기본 스탯: 용사 고르기 전 = SlimeData.csv, 용사를 고르면 = HeroData.csv의 그 용사 행 (+ 용사 강화 포인트)
public class HeroDataPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator HeroPick_SwapsSlimeBaseStats()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    static float Expected(float baseValue, StatType statType) => SlimeManager.Instance.GetTotalStatValue(baseValue, statType);

    IEnumerator Body()
    {
        Slime slime = Slime.Instance;
        SlimeData ksData = slime.statMgr.slimeData;
        Assert.IsNull(slime.statMgr.heroData, "시작 = 용사 없음");
        foreach (StatType st in HeroDataTable.STAT_COLUMNS)
            Assert.AreEqual(Expected(ksData.GetSlimeStat(st)?.value ?? 0f, st), slime.statMgr.statDic[st].initValue, 0.0001f, $"용사 전 {st} = SlimeData");

        // 데이터: HeroData.csv → Resources/HeroData (Knight 행)
        HeroDataTable.HeroData knight = HeroDataTable.Get("Knight");
        Assert.IsNotNull(knight, "HeroData.csv에 Knight 행 없음 (Resources/HeroData 임포트)");

        // 레벨업에 나오는 용사마다 HeroData 행이 있어야 함 (key = 클래스 이름에서 Hero를 뺀 것)
        var setting = SlimeGrowthSetting.Instance;
        foreach (AbilityData d in setting.heroAbilities)
            if (d.prefab is HeroSlimeAbility h)
                Assert.IsNotNull(HeroDataTable.Get(h.HeroDataKey), $"HeroData.csv에 {h.HeroDataKey} 행 없음 ({d.key})");

        // 기사 선택 → 슬라임 기본 스탯 = Knight 행
        AbilityData knightData = setting.heroAbilities.First(d => d.prefab is KnightHero);
        slime.abilityInventory.Add(knightData);
        yield return WaitFrames(2);
        Assert.AreEqual("Knight", slime.statMgr.heroData?.key, "기사 → HeroData Knight");
        foreach (var s in knight.stats)
            Assert.AreEqual(Expected(s.value, s.statType), slime.statMgr.statDic[s.statType].initValue, 0.0001f, $"기사 {s.statType} = HeroData");

        // 값이 다른 용사 행으로 바꾸면 스탯·최대 체력이 바뀜 (체력 증가분만큼 현재 체력도)
        var custom = new HeroDataTable.HeroData { key = "TestHero" };
        custom.stats.Add(new HeroDataTable.HeroStat { statType = StatType.AttackPower, value = 99f });
        custom.stats.Add(new HeroDataTable.HeroStat { statType = StatType.MaxHp, value = 50f });
        float hp = slime.health.CurHp, maxHp = slime.statMgr.MaxHp;
        slime.ApplyHeroBaseStats(custom);
        Assert.AreEqual(Expected(99f, StatType.AttackPower), slime.statMgr.AttackPower, 0.0001f, "공격력 = 용사 행");
        Assert.AreEqual(Expected(50f, StatType.MaxHp), slime.statMgr.MaxHp, 0.0001f, "최대 체력 = 용사 행");
        Assert.AreEqual(hp + (slime.statMgr.MaxHp - maxHp), slime.health.CurHp, 0.0001f, "늘어난 최대 체력만큼 현재 체력");
        Assert.AreEqual(Expected(ksData.GetSlimeStat(StatType.AttackSpeed)?.value ?? 0f, StatType.AttackSpeed), slime.statMgr.statDic[StatType.AttackSpeed].initValue, 0.0001f,
            "용사 행에 없는 스탯 = SlimeData");

        // 되돌리기 (용사 해제)
        slime.ApplyHeroBaseStats(null);
        Assert.AreEqual(Expected(ksData.GetSlimeStat(StatType.AttackPower)?.value ?? 0f, StatType.AttackPower), slime.statMgr.AttackPower, 0.0001f, "해제 = SlimeData");
    }
}
