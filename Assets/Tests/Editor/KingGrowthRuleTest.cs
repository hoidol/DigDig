using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// (2026-11-20) 킹 성장 레벨업 카드 규칙 (LevelUpChoiceRule) + 설정 에셋
public class KingGrowthRuleTest
{
    static AbilityData Make(string key, int maxLevel)
    {
        var d = ScriptableObject.CreateInstance<AbilityData>();
        d.key = key;
        d.maxLevel = maxLevel;
        return d;
    }

    [Test]
    public void StatChoices_AreDistinct_AndSkipMaxLevel()
    {
        var a = Make("A", 5);
        var b = Make("B", 3);
        var c = Make("C", 5);
        var d = Make("D", 5);
        var levels = new Dictionary<AbilityData, int> { { a, 5 }, { b, 1 } }; // A는 최대 레벨
        var rng = new System.Random(1);

        for (int i = 0; i < 50; i++)
        {
            var choices = LevelUpChoiceRule.PickStatChoices(new[] { a, b, c, d }, x => levels.TryGetValue(x, out int lv) ? lv : 0, rng);
            Assert.AreEqual(3, choices.Count);
            Assert.AreEqual(3, choices.Distinct().Count());
            Assert.IsFalse(choices.Contains(a));
        }
    }

    [Test]
    public void StatChoices_FewerCandidates_ReturnsWhatIsLeft()
    {
        var a = Make("A", 1);
        var b = Make("B", 2);
        var choices = LevelUpChoiceRule.PickStatChoices(new[] { a, b }, x => x == a ? 1 : 0, new System.Random(2));
        CollectionAssert.AreEqual(new[] { b }, choices);
    }

    [Test]
    public void FirstLevelUp_OffersRace_ThenStats()
    {
        var races = new[] { Make("R1", 1), Make("R2", 1), Make("R3", 1), Make("R4", 1), Make("R5", 1) };
        var stats = new[] { Make("S1", 5), Make("S2", 5), Make("S3", 5) };
        var rng = new System.Random(3);

        var first = LevelUpChoiceRule.GetChoices(1, false, races, stats, x => 0, rng);
        Assert.AreEqual(3, first.Count);
        Assert.IsTrue(first.All(races.Contains));
        Assert.AreEqual(3, first.Distinct().Count());

        // 여러 레벨이 한 번에 올라도 종족이 없으면 종족
        Assert.IsTrue(LevelUpChoiceRule.GetChoices(4, false, races, stats, x => 0, rng).All(races.Contains));

        var after = LevelUpChoiceRule.GetChoices(2, true, races, stats, x => 0, rng);
        Assert.IsTrue(after.All(stats.Contains));
    }

    [Test]
    public void RaceShapeText_Orc()
    {
        var offs = new[] { new Vector2Int(0, -1), new Vector2Int(-1, 1), new Vector2Int(1, 1) };
        Assert.AreEqual("□■□\n□K□\n■□■", RaceKingAbility.GetShapeText(offs));
    }

    [Test]
    public void Setting_Races_FiveDistinct_ThreeTiles()
    {
        var setting = Resources.Load<KingGrowthSetting>(KingGrowthSetting.RESOURCE_PATH);
        Assert.AreEqual(5, setting.raceAbilities.Length);
        var types = new HashSet<KingRaceType>();
        foreach (var data in setting.raceAbilities)
        {
            Assert.AreEqual(AbilityCategory.Race, data.category, data.key);
            var race = data.prefab as RaceKingAbility;
            Assert.IsNotNull(race, data.key);
            Assert.IsTrue(types.Add(race.raceType), $"종족 중복 {race.raceType}");
            Assert.AreEqual(3, race.tileOffsets.Distinct().Count(), data.key);
            foreach (var o in race.tileOffsets)
                Assert.IsTrue(Mathf.Abs(o.x) <= 1 && Mathf.Abs(o.y) <= 1 && o != Vector2Int.zero, $"{data.key} {o}");
        }
    }

    [Test]
    public void Setting_StatAbilities_HavePrefab()
    {
        var setting = Resources.Load<KingGrowthSetting>(KingGrowthSetting.RESOURCE_PATH);
        Assert.IsNotNull(setting, "Resources/KingGrowthSetting 없음");
        Assert.IsNotEmpty(setting.statAbilities);
        foreach (var data in setting.statAbilities)
        {
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.prefab, $"{data.key} prefab 없음");
            Assert.AreEqual(AbilityCategory.Stat, data.category, data.key);
            Assert.IsFalse(data.GetDescription(1).Contains("{0}"), data.key);
        }
    }
}
