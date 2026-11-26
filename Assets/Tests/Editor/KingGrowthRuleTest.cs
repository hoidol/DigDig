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

    // (2026-11-26) 단계 = Lv.1 용사 → 2 스킬 → 3·4·5 용사 강화 → 6 스킬 → 7·8·9 용사 강화 → 10 스킬 → 11·12 용사 강화. 앞 단계가 먼저
    static AbilityData[] heroes;
    static AbilityData[] boosts;
    static AbilityData[] Heroes => heroes ??= new[] { Make("H1", 5), Make("H2", 5), Make("H3", 5) };
    static AbilityData[] Boosts => boosts ??= new[] { Make("B1", 3), Make("B2", 3), Make("B3", 3) };

    static LevelUpChoiceRule.Input In(int lv, int heroLv, int skills, int boostCount = 0) => new LevelUpChoiceRule.Input
    {
        level = lv, heroLevel = heroLv, heroSkillCount = skills, boostCount = boostCount,
        ownedHero = heroLv > 0 ? Heroes[0] : null,
        heroPool = Heroes, boostPool = Boosts, getLevel = x => 0,
    };

    [TestCase(1, 0, 0, 0, AbilityCategory.Hero)]
    [TestCase(4, 0, 0, 0, AbilityCategory.Hero)]          // 여러 레벨이 밀려도 용사가 먼저
    [TestCase(1, 1, 0, 0, AbilityCategory.Stat)]
    [TestCase(2, 1, 0, 0, AbilityCategory.HeroSkill)]
    [TestCase(3, 1, 0, 0, AbilityCategory.HeroSkill)]     // 스킬 해방이 강화보다 먼저
    [TestCase(3, 1, 1, 0, AbilityCategory.HeroBoost)]
    [TestCase(4, 1, 1, 1, AbilityCategory.HeroBoost)]
    [TestCase(4, 2, 1, 2, AbilityCategory.Stat)]
    [TestCase(5, 1, 1, 2, AbilityCategory.HeroBoost)]
    [TestCase(5, 1, 1, 3, AbilityCategory.Stat)]
    [TestCase(6, 1, 1, 3, AbilityCategory.HeroSkill)]
    [TestCase(7, 1, 2, 3, AbilityCategory.HeroBoost)]
    [TestCase(9, 1, 2, 5, AbilityCategory.HeroBoost)]
    [TestCase(9, 1, 2, 6, AbilityCategory.Stat)]
    [TestCase(10, 1, 2, 6, AbilityCategory.HeroSkill)]
    [TestCase(11, 1, 3, 6, AbilityCategory.HeroBoost)]
    [TestCase(12, 1, 3, 7, AbilityCategory.HeroBoost)]
    [TestCase(12, 1, 3, 8, AbilityCategory.Stat)]
    public void Stage_ByLevel(int lv, int heroLv, int skills, int boostCount, AbilityCategory expected)
    {
        Assert.AreEqual(expected, LevelUpChoiceRule.GetStage(In(lv, heroLv, skills, boostCount)));
    }

    [Test]
    public void Choices_MatchStage()
    {
        var rng = new System.Random(3);
        var hero = LevelUpChoiceRule.GetChoices(In(1, 0, 0), rng);
        Assert.AreEqual(3, hero.Count);
        Assert.AreEqual(3, hero.Distinct().Count());
        CollectionAssert.AreEqual(new[] { Heroes[0] }, LevelUpChoiceRule.GetChoices(In(2, 1, 0), rng), "스킬 해방 = 고른 용사 한 장");
        var boost = LevelUpChoiceRule.GetChoices(In(3, 1, 1), rng);
        Assert.AreEqual(3, boost.Count);
        Assert.AreEqual(3, boost.Distinct().Count());
        Assert.IsTrue(boost.All(x => Boosts.Contains(x)), "용사 강화 = 강화 카드만 (용사 카드 X)");
        Assert.IsEmpty(LevelUpChoiceRule.GetChoices(In(4, 2, 1, 2), rng), "단계 카드 없음");
        Assert.AreEqual("용사 선택", LevelUpChoiceRule.GetStageName(AbilityCategory.Hero));
        Assert.AreEqual("스킬 해방", LevelUpChoiceRule.GetStageName(AbilityCategory.HeroSkill));
        Assert.AreEqual("용사 강화", LevelUpChoiceRule.GetStageName(AbilityCategory.HeroBoost));
        Assert.AreEqual(2, LevelUpChoiceRule.GetSkillUnlockLevel(0));
        Assert.AreEqual(6, LevelUpChoiceRule.GetSkillUnlockLevel(1));
        Assert.AreEqual(10, LevelUpChoiceRule.GetSkillUnlockLevel(2));
    }

    // (2026-11-26) 필드 최대 미니언 수 +1 = Lv.7에만
    [TestCase(1, 0)]
    [TestCase(6, 0)]
    [TestCase(7, 1)]
    [TestCase(12, 1)]
    public void MinionCountAdd_ByLevel(int heroLevel, int expected)
    {
        Assert.AreEqual(expected, LevelUpChoiceRule.GetMinionCountAdd(heroLevel));
    }

    // 용사 강화 후보: 최대 레벨인 카드는 빠짐 (용사 카드는 heroLevel 기준)
    [Test]
    public void Boost_ExcludesMax()
    {
        var input = In(3, 5, 1);
        input.getLevel = d => d == Boosts[0] ? 3 : 0;
        var cands = LevelUpChoiceRule.GetBoostCandidates(input);
        CollectionAssert.AreEquivalent(new[] { Boosts[1], Boosts[2] }, cands);
    }

    [Test]
    public void HeroSkill_ParseValues()
    {
        var s = HeroDataTable.HeroSkill.Parse("{0}초 {1}%", "5/7, 3 ,0.5/1");
        CollectionAssert.AreEqual(new[] { 5f, 3f, 0.5f }, s.values);
        CollectionAssert.AreEqual(new[] { 7f, 3f, 1f }, s.enhancedValues, "/가 없으면 강화해도 같음");
    }

    // (2026-11-25) 용사 강화 3장 = 정예 칸 추가가 아닌 카드 1장씩 + 남은 자리 정예 칸 추가
    [Test]
    public void HeroBoost_FillWithExpand()
    {
        var charge = Make("Charge", 3);
        charge.category = AbilityCategory.HeroBoost;
        var expand = Make("Expand", 3);
        expand.category = AbilityCategory.TileExpand;
        var rng = new System.Random(7);
        for (int i = 0; i < 20; i++)
        {
            var both = LevelUpChoiceRule.PickHeroBoostChoices(new[] { charge, expand }, rng);
            Assert.AreEqual(3, both.Count);
            Assert.AreEqual(1, both.Count(x => x == charge));
            Assert.AreEqual(2, both.Count(x => x == expand));
        }
        CollectionAssert.AreEqual(new[] { expand, expand, expand }, LevelUpChoiceRule.PickHeroBoostChoices(new[] { expand }, rng), "충전 가속 최대 → 정예 칸 3장");
        CollectionAssert.AreEqual(new[] { charge }, LevelUpChoiceRule.PickHeroBoostChoices(new[] { charge }, rng), "정예 칸 최대 → 충전 가속만");
    }

    // (2026-11-25) HeroData.csv: 용사 레벨 L 스탯 = 기본 + Add × (L-1), 정예병 칸 버프 = Init + Add × (L-1)
    [Test]
    public void HeroData_LevelStats()
    {
        DataTableImporter.ImportChanged(); // CSV → Resources/HeroData
        foreach (string key in new[] { "Knight", "Ranger", "Necromancer" })
        {
            var data = HeroDataTable.Get(key);
            Assert.IsNotNull(data, key);
            Assert.IsTrue(data.TryGetStat(StatType.AttackPower, out float atk1), key);
            Assert.IsTrue(data.TryGetStat(StatType.AttackPower, 3, out float atk3), key);
            Assert.AreEqual(atk1 + data.GetAdd(StatType.AttackPower) * 2, atk3, 0.001f, key);
            Assert.IsTrue(data.HasEliteBuff, $"{key} 정예병 칸 버프 값");
            Assert.AreEqual(data.eliteBuffInit + data.eliteBuffAdd * 4, data.GetEliteBuffValue(5), 0.001f, key);
            StringAssert.Contains("{0}", data.eliteBuffDesc, $"{key} EliteBuffDesc");
            // (2026-11-25) 스킬 1~3 설명·값 ("기본/강화")
            Assert.AreEqual(3, data.skills.Count, key);
            for (int i = 0; i < 3; i++)
            {
                Assert.IsNotEmpty(data.skills[i].desc, $"{key} Skill{i + 1}");
                Assert.IsNotEmpty(data.skills[i].values, $"{key} Skill{i + 1}Value");
                Assert.AreEqual(data.skills[i].values.Count, data.skills[i].enhancedValues.Count);
            }
            Assert.Greater(data.skills[0].enhancedValues[0], data.skills[0].values[0], $"{key} 1단계 강화 값");
        }
    }

    // 용사 카드 정예병 칸 = 킹 주변 8칸 중 서로 다른 2칸
    [Test]
    public void HeroTiles_TwoDistinctAround()
    {
        var rng = new System.Random(5);
        for (int i = 0; i < 50; i++)
        {
            var tiles = LevelUpChoiceRule.PickHeroTiles(rng);
            Assert.AreEqual(2, tiles.Distinct().Count());
            Assert.IsTrue(tiles.All(t => LevelUpChoiceRule.AROUND_OFFSETS.Contains(t)));
        }
    }

    // 용사 3명 + 용사 강화 카드 5장, 설명 빈칸 없음
    [Test]
    public void Setting_Heroes_Three()
    {
        var setting = Resources.Load<KingGrowthSetting>(KingGrowthSetting.RESOURCE_PATH);
        Assert.AreEqual(3, setting.heroAbilities.Length);
        var types = new HashSet<KingHeroType>();
        foreach (var data in setting.heroAbilities)
        {
            Assert.AreEqual(AbilityCategory.Hero, data.category, data.key);
            var hero = data.prefab as HeroKingAbility;
            Assert.IsNotNull(hero, data.key);
            Assert.IsTrue(types.Add(hero.heroType), $"용사 중복 {hero.heroType}");
            Assert.AreEqual(1, data.maxLevel, data.key); // (2026-11-25) 용사 카드는 한 번만 (강화는 레벨업 자동)
            Assert.AreEqual(2, hero.skills.Length, data.key);
            Assert.IsFalse(string.IsNullOrEmpty(hero.energyName), data.key);
            for (int s = 0; s < HeroKingAbility.SKILL_COUNT; s++)
                Assert.IsFalse(string.IsNullOrEmpty(hero.GetSkillName(s)), $"{data.key} 스킬 {s}");
            Assert.IsFalse(data.GetDescription(1).Contains("{"), data.key);
        }
        CollectionAssert.AreEquivalent(new[] { KingHeroType.Knight, KingHeroType.Ranger, KingHeroType.Necromancer }, types);
        var necro = setting.heroAbilities.Select(d => d.prefab).OfType<NecromancerHero>().First();
        Assert.AreEqual(AttackType.Chaos, necro.kingAttackType);

        // (2026-11-26) 용사 강화 = 정예 칸 추가 (최대 5) · 충전 가속 (×0.8, 최대 5) · 에너지 보유 증가 (+1, 최대 3) · 1·2단계 스킬 강화 (최대 1)
        Assert.AreEqual(5, setting.heroBoostAbilities.Length);
        var chargeCard = setting.heroBoostAbilities.Single(d => d.prefab is HeroEnergyKingAbility e && e.boostType == HeroEnergyBoostType.ChargeTime);
        Assert.AreEqual(5, chargeCard.maxLevel);
        var chargeAbility = (HeroEnergyKingAbility)chargeCard.prefab;
        Assert.AreEqual(0.8f, chargeAbility.GetChargeTimeMul(1), 0.001f, "충전 가속 1레벨 = ×0.8");
        Assert.AreEqual(0.32768f, chargeAbility.GetChargeTimeMul(5), 0.001f, "충전 가속 5레벨 = ×0.8^5");
        Assert.GreaterOrEqual(GameSetting.HERO_ENERGY_CHARGE_TIME * chargeAbility.GetChargeTimeMul(5), GameSetting.HERO_MIN_CHARGE_TIME, "5레벨까지 최소 충전 시간에 안 막힘");
        Assert.AreEqual(3, setting.heroBoostAbilities.Single(d => d.prefab is HeroEnergyKingAbility e && e.boostType == HeroEnergyBoostType.MaxCharges).maxLevel);
        for (int idx = 0; idx < 2; idx++)
        {
            var sk = setting.heroBoostAbilities.Single(d => d.prefab is HeroSkillEnhanceKingAbility s && s.skillIndex == idx);
            Assert.AreEqual(1, sk.maxLevel, sk.key);
            Assert.IsFalse(LevelUpChoiceRule.IsBoostUnlocked(sk, idx), $"{sk.key}: 스킬 해방 전엔 안 나옴");
            Assert.IsTrue(LevelUpChoiceRule.IsBoostUnlocked(sk, idx + 1), $"{sk.key}: 스킬 해방 후 나옴");
        }
        var expandCard = setting.heroBoostAbilities.Single(d => d.prefab is EliteAreaExpandKingAbility);
        Assert.AreEqual(AbilityCategory.TileExpand, expandCard.category);
        Assert.AreEqual(5, expandCard.maxLevel);
        foreach (var data in setting.heroBoostAbilities)
            Assert.IsFalse(data.GetDescription(1).Contains("{"), data.key);
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





}
