using System.Linq;
using NUnit.Framework;

// (2026-11-24) 슬라임 레벨 스탯: 유저 레벨 1당 1포인트, 묶음 해금 레벨·묶음 상한, 스탯 계산 (씬 없이)
public class SlimeLevelStatRuleTest
{
    // (2026-11-26) 묶음 = 레벨 구간: Lv.1~5 포인트 → 1번 묶음, Lv.6~10 → 2번 … (묶음끼리 안 섞임)
    [Test]
    public void TotalPoints_OnePerLevel()
    {
        Assert.AreEqual(0, UserSlimeManager.GetTotalPoints(0));
        Assert.AreEqual(5, UserSlimeManager.GetTotalPoints(5));
        Assert.AreEqual(30, UserSlimeManager.GetTotalPoints(30));
    }

    [Test]
    public void TierEarnedPoints_ByLevelRange()
    {
        Assert.AreEqual(0, UserSlimeManager.GetTierEarnedPoints(0, 1, 5));
        Assert.AreEqual(1, UserSlimeManager.GetTierEarnedPoints(1, 1, 5), "Lv.1 = 1번 묶음 1포인트");
        Assert.AreEqual(5, UserSlimeManager.GetTierEarnedPoints(5, 1, 5));
        Assert.AreEqual(5, UserSlimeManager.GetTierEarnedPoints(9, 1, 5), "구간이 끝나면 더 안 늘어남");
        Assert.AreEqual(0, UserSlimeManager.GetTierEarnedPoints(5, 6, 5), "Lv.5엔 2번 묶음 포인트 없음");
        Assert.AreEqual(2, UserSlimeManager.GetTierEarnedPoints(7, 6, 5), "Lv.6·7 = 2번 묶음 2포인트");
    }

    [Test]
    public void CanAddPoint_Order()
    {
        Assert.AreEqual(SlimeStatPointResult.Locked, UserSlimeManager.CanAddPoint(5, 6, 0, 5), "레벨 미달 = 잠김");
        Assert.AreEqual(SlimeStatPointResult.Success, UserSlimeManager.CanAddPoint(1, 1, 0, 5), "Lv.1부터 1번 묶음");
        Assert.AreEqual(SlimeStatPointResult.NoPoint, UserSlimeManager.CanAddPoint(3, 1, 3, 5), "Lv.3 = 1번 묶음 3포인트까지");
        Assert.AreEqual(SlimeStatPointResult.Success, UserSlimeManager.CanAddPoint(5, 1, 4, 5));
        Assert.AreEqual(SlimeStatPointResult.TierFull, UserSlimeManager.CanAddPoint(9, 1, 5, 5), "묶음 합계 5까지");
        Assert.AreEqual(SlimeStatPointResult.NoPoint, UserSlimeManager.CanAddPoint(7, 6, 2, 5), "Lv.7 = 2번 묶음 2포인트까지");
    }

    [Test]
    public void CalcStat_AddAndPercent()
    {
        Assert.AreEqual(17f, UserSlimeManager.CalcStat(15f, 2f, 0f), 0.0001f, "공격력 +1 × 2");
        Assert.AreEqual(65f * 1.04f, UserSlimeManager.CalcStat(65f, 0f, 4f), 0.0001f, "공속 +2% × 2 = 기본 × 1.04");
    }

    [Test]
    public void Setting_Valid()
    {
        var table = UnityEngine.ScriptableObject.CreateInstance<TranslateDataTable>();
        table.LoadData();
        var keys = new System.Collections.Generic.HashSet<string>(table.rows.Select(r => r.key));
        UnityEngine.Object.DestroyImmediate(table);

        var tiers = SlimeLevelStatSetting.Tiers;
        Assert.Greater(tiers.Length, 0);
        for (int i = 0; i < tiers.Length; i++)
        {
            Assert.Greater(tiers[i].maxPoints, 0, $"{i}번 묶음 상한");
            Assert.LessOrEqual(tiers[i].options.Length, LevelStatContainer.SLOTS_PER_TIER, $"{i}번 묶음 스탯 수 ≤ 칸 수");
            if (i > 0)
                Assert.AreEqual(tiers[i - 1].unlockLevel + tiers[i - 1].maxPoints, tiers[i].unlockLevel, "묶음 구간은 빈틈·겹침 없이 이어짐");
            foreach (var o in tiers[i].options)
            {
                // 번역 key는 Translate.csv 기준 (에셋은 Play 때 임포트되므로 CSV를 직접 읽음)
                if (o.kind == SlimeLevelStatSetting.OptionKind.Ability)
                {
                    Assert.IsTrue(keys.Contains($"HeroUpgrade.{o.ability}"), $"HeroUpgrade.{o.ability} 번역 없음");
                    Assert.IsTrue(keys.Contains($"HeroUpgrade.{o.ability}.Desc"), $"HeroUpgrade.{o.ability}.Desc 번역 없음");
                }
                else
                    Assert.IsTrue(keys.Contains($"{o.statType}_title"), $"{o.statType}_title 번역 없음");
                Assert.Greater(o.ValueAt(1), 0f, $"{i}번 묶음 1포인트 값");
                Assert.GreaterOrEqual(o.ValueAt(5), o.ValueAt(1), "5포인트까지 찍을 수 있고 값이 줄지 않음");
            }
        }
        // 첫 묶음 = Lv.1~5 공격력 +1 / 공속 +2% / 공격 범위 +20(표시 ×100)
        var first = tiers[0];
        Assert.AreEqual(1, first.unlockLevel);
        Assert.AreEqual(5, first.maxPoints);
        Assert.AreEqual("+1", first.options.First(o => o.statType == StatType.AttackPower).GetValueText(1));
        Assert.AreEqual("+2%", first.options.First(o => o.statType == StatType.AttackSpeed).GetValueText(1));
        Assert.AreEqual("+20", first.options.First(o => o.statType == StatType.AttackRange).GetValueText(1));
    }

    // (2026-11-24) 용사 강화: 1번 묶음(Lv.1~5) = 스탯, 2번(Lv.6~)부터 능력
    [Test]
    public void Tiers_StatsThenAbilities()
    {
        var tiers = SlimeLevelStatSetting.Tiers;
        Assert.IsTrue(tiers[0].options.All(o => o.kind == SlimeLevelStatSetting.OptionKind.Stat), "Lv.1~5 = 스탯");
        Assert.IsTrue(tiers.Where(t => t.unlockLevel >= 6).SelectMany(t => t.options).Any(o => o.kind == SlimeLevelStatSetting.OptionKind.Ability), "Lv.6부터 능력");
        var lv10 = tiers.First(t => t.unlockLevel == 6);
        CollectionAssert.AreEquivalent(new[] { HeroUpgradeAbility.EliteMinionAttack, HeroUpgradeAbility.EliteMinionAttackSpeed, HeroUpgradeAbility.EliteMinionSkillCooldown },
            lv10.options.Select(o => o.ability), "Lv.6~10 = 정예병");
        var lv30 = tiers.First(t => t.unlockLevel == 26);
        CollectionAssert.AreEqual(new[] { HeroUpgradeAbility.StartDiceCoin, HeroUpgradeAbility.MinionCostDownChance, HeroUpgradeAbility.FirstOfferLv2Chance },
            lv30.options.Select(o => o.ability), "Lv.26~30 = 준비 (판 시작)");
    }

    [Test]
    public void ValueAt_PerPointTable()
    {
        var dice = SlimeLevelStatSetting.StatOption.Ability(HeroUpgradeAbility.EliteBossDiceChance, 1f, "%", 5, 6, 7, 8, 9);
        Assert.AreEqual(0f, dice.ValueAt(0));
        Assert.AreEqual(5f, dice.ValueAt(1));
        Assert.AreEqual(9f, dice.ValueAt(5));
        Assert.AreEqual(9f, dice.ValueAt(7), "표보다 많으면 마지막 값");
        var reroll = SlimeLevelStatSetting.StatOption.Ability(HeroUpgradeAbility.BreakRerollChance, 1f, "%", 3, 4, 5, 6, 7);
        Assert.AreEqual(3f, reroll.ValueAt(1));
        Assert.AreEqual(7f, reroll.ValueAt(5));
        var linear = SlimeLevelStatSetting.StatOption.Ability(HeroUpgradeAbility.FirstOfferLv2Chance, 15f, "%");
        Assert.AreEqual(75f, linear.ValueAt(5), "선형 = 포인트 × 값");
        Assert.AreEqual("+75%", linear.GetValueText(5));
    }

    [Test]
    public void Conditions_FormAndType()
    {
        var forms = new System.Collections.Generic.List<MinionTag> { MinionTag.Shooter, MinionTag.Zone, MinionTag.Orbit, MinionTag.Support, MinionTag.Special | MinionTag.Chance };
        Assert.IsTrue(HeroUpgradeSystem.IsFormVariety(forms));
        forms[4] = MinionTag.Shooter;
        Assert.IsFalse(HeroUpgradeSystem.IsFormVariety(forms), "형태 겹침");
        Assert.IsTrue(HeroUpgradeSystem.IsPureType(Enumerable.Repeat(AttackType.Magic, 5).ToList()));
        Assert.IsFalse(HeroUpgradeSystem.IsPureType(new System.Collections.Generic.List<AttackType> { AttackType.Magic, AttackType.Magic, AttackType.Magic, AttackType.Magic, AttackType.Physic }));
        Assert.IsTrue(HeroUpgradeSystem.Roll(5f, 0.04f));
        Assert.IsFalse(HeroUpgradeSystem.Roll(5f, 0.05f));
        Assert.IsFalse(HeroUpgradeSystem.Roll(0f, 0f), "0%는 항상 실패");
        Assert.AreEqual(0.55f, HeroUpgradeSystem.ApplyResistEase(0.5f, 5f), 0.0001f, "내성 +5%p");
        Assert.AreEqual(1f, HeroUpgradeSystem.ApplyResistEase(0.9f, 25f), 0.0001f, "최대 100%");
        Assert.AreEqual(1.5f, HeroUpgradeSystem.ApplyResistEase(1.5f, 25f), 0.0001f, "약점엔 적용 안 함");
    }

    [Test]
    public void Weakness_OnlyAboveOne()
    {
        HeroUpgradeSystem.End();
        Assert.AreEqual(1.5f, HeroUpgradeSystem.ApplyWeakness(1.5f), 0.0001f, "조건이 안 되면 그대로");
    }
}
