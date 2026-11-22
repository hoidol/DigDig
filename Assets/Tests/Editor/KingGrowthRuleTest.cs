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

    // (2026-11-21) 단계 = Lv.1 종족 → Lv.5 종족 강화(버프 칸 확장) → Lv.10 직업 → Lv.15 패시브, 앞 단계가 먼저
    static AbilityData[] races, jobs, passives, stats;
    static AbilityData expand;
    static AbilityData[] Races => races ??= new[] { Make("R1", 1), Make("R2", 1), Make("R3", 1), Make("R4", 1), Make("R5", 1) };
    static AbilityData[] Jobs => jobs ??= new[] { Make("J1", 1), Make("J2", 1), Make("J3", 1), Make("J4", 1), Make("J5", 1) };
    static AbilityData[] Passives => passives ??= new[] { Make("P1", 3), Make("P2", 3), Make("P3", 3) };
    static AbilityData[] Stats => stats ??= new[] { Make("S1", 5), Make("S2", 5), Make("S3", 5) };
    static AbilityData Expand => expand != null ? expand : (expand = Make("E", 1));

    static LevelUpChoiceRule.Input In(int lv, bool race, bool expand, bool job, bool passive) => new LevelUpChoiceRule.Input
    {
        level = lv, hasRace = race, hasTileExpand = expand, hasJob = job, hasPassive = passive,
        racePool = Races, jobPool = Jobs, passivePool = Passives, statPool = Stats, tileExpand = Expand, getLevel = x => 0,
    };

    [TestCase(1, false, false, false, false, AbilityCategory.Race)]
    [TestCase(4, false, false, false, false, AbilityCategory.Race)]       // 여러 레벨이 밀려도 종족이 먼저
    [TestCase(2, true, false, false, false, AbilityCategory.Stat)]
    [TestCase(4, true, false, false, false, AbilityCategory.Stat)]
    [TestCase(5, true, false, false, false, AbilityCategory.TileExpand)]  // 종족 강화
    [TestCase(8, true, false, false, false, AbilityCategory.TileExpand)]
    [TestCase(9, true, true, false, false, AbilityCategory.Stat)]
    [TestCase(10, true, true, false, false, AbilityCategory.Job)]
    [TestCase(12, true, false, false, false, AbilityCategory.TileExpand)] // 강화가 직업보다 먼저
    [TestCase(14, true, true, true, false, AbilityCategory.Stat)]
    [TestCase(15, true, true, true, false, AbilityCategory.Passive)]
    [TestCase(15, true, true, false, false, AbilityCategory.Job)]         // 직업이 패시브보다 먼저
    [TestCase(16, true, true, true, true, AbilityCategory.Stat)]
    public void Stage_ByLevel(int lv, bool race, bool expand, bool job, bool passive, AbilityCategory expected)
    {
        Assert.AreEqual(expected, LevelUpChoiceRule.GetStage(In(lv, race, expand, job, passive)));
    }

    [Test]
    public void Choices_MatchStage()
    {
        var rng = new System.Random(3);
        var race = LevelUpChoiceRule.GetChoices(In(1, false, false, false, false), rng);
        Assert.AreEqual(3, race.Count);
        Assert.AreEqual(3, race.Distinct().Count());
        Assert.IsTrue(race.All(Races.Contains));
        var expand = LevelUpChoiceRule.GetChoices(In(5, true, false, false, false), rng);
        Assert.AreEqual(3, expand.Count);
        Assert.IsTrue(expand.All(x => x == Expand), "종족 강화 = 같은 카드 3장");
        Assert.IsTrue(LevelUpChoiceRule.GetChoices(In(10, true, true, false, false), rng).All(Jobs.Contains));
        Assert.IsTrue(LevelUpChoiceRule.GetChoices(In(3, true, false, false, false), rng).All(Stats.Contains));
        Assert.AreEqual("종족 선택", LevelUpChoiceRule.GetStageName(AbilityCategory.Race));
        Assert.AreEqual("종족 강화", LevelUpChoiceRule.GetStageName(AbilityCategory.TileExpand));
        Assert.AreEqual("직업 선택", LevelUpChoiceRule.GetStageName(AbilityCategory.Job));
    }

    // 버프 칸 확장 추가 칸 = 지금 버프 칸을 뺀 나머지, 카드끼리 다름
    [Test]
    public void ExpandTiles_ExcludeCurrentTiles_AndDistinct()
    {
        var race = new List<Vector2Int> { new Vector2Int(0, -1), new Vector2Int(-1, 1), new Vector2Int(1, 1) };
        var rng = new System.Random(5);
        for (int i = 0; i < 50; i++)
        {
            var tiles = LevelUpChoiceRule.PickExpandTiles(race, 3, rng);
            Assert.AreEqual(3, tiles.Count);
            Assert.AreEqual(3, tiles.Distinct().Count());
            foreach (var t in tiles)
            {
                Assert.IsFalse(race.Contains(t), $"종족 칸 {t}");
                Assert.IsTrue(t != Vector2Int.zero && Mathf.Abs(t.x) <= 1 && Mathf.Abs(t.y) <= 1);
            }
        }
    }

    [Test]
    public void Setting_Jobs_FiveDistinct()
    {
        var setting = Resources.Load<KingGrowthSetting>(KingGrowthSetting.RESOURCE_PATH);
        Assert.AreEqual(5, setting.jobAbilities.Length);
        var types = new HashSet<KingJobType>();
        foreach (var data in setting.jobAbilities)
        {
            Assert.AreEqual(AbilityCategory.Job, data.category, data.key);
            var job = data.prefab as JobKingAbility;
            Assert.IsNotNull(job, data.key);
            Assert.IsTrue(types.Add(job.jobType), $"직업 중복 {job.jobType}");
            Assert.IsNotEmpty(job.tileBuffs, data.key);
        }
        Assert.IsNotNull(setting.tileExpandAbility, "버프 칸 확장 카드 없음");
        Assert.AreEqual(AbilityCategory.TileExpand, setting.tileExpandAbility.category);
        Assert.IsInstanceOf<BuffTileExpandKingAbility>(setting.tileExpandAbility.prefab);
    }

    // Lv.15 = 내 종족 패시브, 고른 뒤엔 능력치 카드에 강화 카드로 섞임 (최대 레벨이면 빠짐)
    [Test]
    public void Level15_OffersRacePassive_ThenUpgradeMixed()
    {
        var passives = new[] { Make("P1", 3), Make("P2", 3), Make("P3", 3) };
        var stats = new[] { Make("S1", 5), Make("S2", 5), Make("S3", 5) };
        var rng = new System.Random(7);
        LevelUpChoiceRule.Input Input(int lv, bool hasPassive, AbilityData owned, System.Func<AbilityData, int> getLevel) => new LevelUpChoiceRule.Input
        {
            level = lv, hasRace = true, hasJob = true, hasTileExpand = true, hasPassive = hasPassive,
            passivePool = passives, statPool = stats, ownedPassive = owned, getLevel = getLevel,
        };

        Assert.IsTrue(LevelUpChoiceRule.GetChoices(Input(14, false, null, x => 0), rng).All(stats.Contains), "Lv.14는 능력치");
        var p = LevelUpChoiceRule.GetChoices(Input(15, false, null, x => 0), rng);
        CollectionAssert.AreEquivalent(passives, p, "Lv.15 = 종족 패시브 3개");

        // 고른 패시브(P1 Lv.1)가 능력치 카드 후보에 들어감 → 여러 번 뽑으면 한 번은 나옴
        bool seen = false;
        for (int i = 0; i < 50; i++)
        {
            var c = LevelUpChoiceRule.GetChoices(Input(16, true, passives[0], x => x == passives[0] ? 1 : 0), rng);
            Assert.IsFalse(c.Contains(passives[1]) || c.Contains(passives[2]), "고르지 않은 패시브는 안 나옴");
            seen |= c.Contains(passives[0]);
        }
        Assert.IsTrue(seen, "패시브 강화 카드가 섞여 나와야 함");
        for (int i = 0; i < 30; i++)
            Assert.IsFalse(LevelUpChoiceRule.GetChoices(Input(17, true, passives[0], x => x == passives[0] ? 3 : 0), rng).Contains(passives[0]), "최대 레벨이면 안 나옴");
    }

    [Test]
    public void Setting_Passives_ThreePerRace_DescFilled()
    {
        var setting = Resources.Load<KingGrowthSetting>(KingGrowthSetting.RESOURCE_PATH);
        Assert.AreEqual(15, setting.passiveAbilities.Length);
        foreach (KingRaceType race in System.Enum.GetValues(typeof(KingRaceType)))
        {
            var list = setting.GetRacePassives(race);
            Assert.AreEqual(3, list.Count, $"{race} 패시브 수");
            Assert.IsTrue(list.Any(d => ((PassiveKingAbility)d.prefab).kind == PassiveKind.Buff), $"{race} 버프형 없음");
            Assert.IsTrue(list.Any(d => ((PassiveKingAbility)d.prefab).kind == PassiveKind.Attack), $"{race} 공격형 없음");
        }
        foreach (var d in setting.passiveAbilities)
        {
            Assert.AreEqual(AbilityCategory.Passive, d.category, d.key);
            Assert.AreEqual(3, d.maxLevel, d.key);
            for (int lv = 1; lv <= 3; lv++)
            {
                string desc = d.GetDescription(lv);
                Assert.IsFalse(desc.Contains("{"), $"{d.key} Lv.{lv} 설명 빈칸: {desc}");
            }
        }
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
