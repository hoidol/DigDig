using NUnit.Framework;

// 기획 규칙 테스트 (CLAUDE.md 기준)
public class WaveTypeTest
{
    // waveIdx는 0부터, wave 번호 = waveIdx + 1
    [TestCase(0, WaveType.Battle)]
    [TestCase(3, WaveType.Battle)]
    [TestCase(4, WaveType.Elite)]   // 5번째
    [TestCase(9, WaveType.Elite)]   // 10번째
    [TestCase(14, WaveType.Elite)]  // 15번째
    [TestCase(19, WaveType.Boss)]   // 20번째: 5의 배수지만 보스 우선
    [TestCase(20, WaveType.Battle)]
    [TestCase(24, WaveType.Elite)]
    [TestCase(39, WaveType.Boss)]
    [TestCase(99, WaveType.Boss)]
    public void GetWaveType(int waveIdx, WaveType expected)
    {
        Assert.AreEqual(expected, GameManager.GetWaveType(waveIdx));
    }

    // 스킵: wave 시작 후 SKIP_ENABLE_TIME(20)초부터 (소환이 끝나기 전이라 다음 wave와 겹침), 보스 wave는 불가
    [TestCase(WaveType.Battle, 0f, false)]
    [TestCase(WaveType.Battle, 19.9f, false)]
    [TestCase(WaveType.Battle, 20f, true)]
    [TestCase(WaveType.Elite, 25f, true)]
    [TestCase(WaveType.Boss, 50f, false)]
    public void CanSkipWave(WaveType waveType, float elapsed, bool expected)
    {
        Assert.AreEqual(expected, GameManager.CanSkipWave(waveType, elapsed));
        Assert.Less(GameSetting.SKIP_ENABLE_TIME, GameSetting.WAVE_SPAWN_TIME, "스킵 가능 시점이 소환 종료보다 앞이어야 겹침");
    }

    // 3 wave마다 Ethereal → Armored 번갈아, 나머지는 Normal (DEFENCE_WAVE_INTERVAL = 3)
    [TestCase(0, DefenceType.Normal)]
    [TestCase(1, DefenceType.Normal)]
    [TestCase(2, DefenceType.Ethereal)]  // 3번째
    [TestCase(3, DefenceType.Normal)]
    [TestCase(5, DefenceType.Armored)]   // 6번째
    [TestCase(8, DefenceType.Ethereal)]  // 9번째
    [TestCase(11, DefenceType.Armored)]  // 12번째
    [TestCase(19, DefenceType.Normal)]   // 20번째 (보스 wave)
    public void GetWaveDefenceType(int waveIdx, DefenceType expected)
    {
        Assert.AreEqual(GameSetting.DEFENCE_WAVE_INTERVAL == 3, true, "간격이 바뀌면 이 기대값도 고칠 것");
        Assert.AreEqual(expected, GameManager.GetWaveDefenceType(waveIdx));
    }

    // 기본 배율 (Resources/DefenceInfo 에셋이 없을 때. 에셋 값은 DefenceTypePlayTest가 실제 전투로 확인)
    [TestCase(AttackType.Physic, DefenceType.Armored, 0.5f)]
    [TestCase(AttackType.Magic, DefenceType.Armored, 1.5f)]
    [TestCase(AttackType.Physic, DefenceType.Ethereal, 1.5f)]
    [TestCase(AttackType.Magic, DefenceType.Ethereal, 0.5f)]
    [TestCase(AttackType.AllRounder, DefenceType.Armored, 1f)]
    public void DefenceDefaultMultiplier(AttackType attack, DefenceType defence, float expected)
    {
        Assert.AreEqual(expected, DefenceInfo.GetDefault(attack, defence), 0.0001f);
    }

    // 아군 최소 피해: 1 미만이면 1, 원래 피해 0은 0
    [TestCase(3f, 0.6f, 1f)]
    [TestCase(3f, 0f, 1f)]
    [TestCase(5f, 2.5f, 2.5f)]
    [TestCase(0f, 0f, 0f)]
    public void AllyMinDamage(float baseDamage, float finalDamage, float expected)
    {
        Assert.AreEqual(expected, AllyUnitDamageData.GetMinGuaranteedDamage(baseDamage, finalDamage), 0.0001f);
    }
}

public class StageUnlockTest
{
    // 낮은 난이도에서 wave 100(waveIdx 99) 이상 도달해야 다음 난이도 해금
    [TestCase(0, false)]
    [TestCase(49, false)]
    [TestCase(98, false)]  // wave 99
    [TestCase(99, true)]   // wave 100
    [TestCase(150, true)]
    public void IsUnlockWave(int lowDifficultyMaxWave, bool expected)
    {
        Assert.AreEqual(expected, StageData.IsUnlockWave(lowDifficultyMaxWave));
    }
}

public class DrawItemChanceTest
{
    // 처음 100%, 성공할 때마다 -5%, 최소 10%
    [TestCase(0, 100)]
    [TestCase(1, 95)]
    [TestCase(10, 50)]
    [TestCase(18, 10)]
    [TestCase(19, 10)]
    [TestCase(100, 10)]
    public void GetDrawChance(int drawCount, int expected)
    {
        Assert.AreEqual(expected, DrawItemButton.GetDrawChance(drawCount));
    }
}

public class KSUpgradePriceTest
{
    // 가격(lv → lv+1) = (BasePrice + PriceAddPerLv × lv) × PriceMulPerLv^lv (반올림)
    [Test]
    public void Linear()
    {
        var info = new KSUpgradeInfo { basePrice = 100, priceAddPerLv = 50, priceMulPerLv = 1f };
        Assert.AreEqual(100, info.GetPrice(0));
        Assert.AreEqual(150, info.GetPrice(1));
        Assert.AreEqual(600, info.GetPrice(10));
    }

    [Test]
    public void Exponential()
    {
        var info = new KSUpgradeInfo { basePrice = 100, priceAddPerLv = 0, priceMulPerLv = 1.5f };
        Assert.AreEqual(100, info.GetPrice(0));
        Assert.AreEqual(150, info.GetPrice(1));
        Assert.AreEqual(225, info.GetPrice(2));
        Assert.AreEqual(338, info.GetPrice(3)); // 337.5 → 반올림
    }

    [Test]
    public void LinearAndExponential()
    {
        var info = new KSUpgradeInfo { basePrice = 100, priceAddPerLv = 10, priceMulPerLv = 2f };
        Assert.AreEqual(100, info.GetPrice(0));
        Assert.AreEqual(220, info.GetPrice(1)); // 110 × 2
        Assert.AreEqual(480, info.GetPrice(2)); // 120 × 4
    }

    [Test]
    public void MaxLv()
    {
        var unlimited = new KSUpgradeInfo { maxLv = 0 };
        Assert.IsFalse(unlimited.IsMaxLv(9999));

        var limited = new KSUpgradeInfo { maxLv = 5 };
        Assert.IsFalse(limited.IsMaxLv(4));
        Assert.IsTrue(limited.IsMaxLv(5));
    }

    [Test]
    public void Value_IsAdditive()
    {
        var info = new KSUpgradeInfo { valuePerLv = 2.5f };
        Assert.AreEqual(0f, info.GetValue(0));
        Assert.AreEqual(25f, info.GetValue(10));
    }
}

public class MergeRuleTest
{
    const int MAX = MinionData.MAX_MERGE_LEVEL;

    [Test]
    public void SameKeySameLv_CanMerge()
    {
        Assert.IsTrue(Minion.CanMerge("Ice", 0, 1, "Ice", 0, 1));
        Assert.IsTrue(Minion.CanMerge("Ice", MAX - 1, 1, "Ice", MAX - 1, 1));
    }

    [Test]
    public void DifferentKey_CannotMerge()
    {
        Assert.IsFalse(Minion.CanMerge("Ice", 0, 1, "Flame", 0, 1));
    }

    [Test]
    public void DifferentLv_CannotMerge()
    {
        Assert.IsFalse(Minion.CanMerge("Ice", 0, 1, "Ice", 1, 1));
    }

    [Test]
    public void MaxLv_CannotMerge()
    {
        Assert.IsFalse(Minion.CanMerge("Ice", MAX, 1, "Ice", MAX, 1));
    }

    [Test]
    public void Sacrifice_AnyKeySameLv_CanMerge()
    {
        Assert.IsTrue(SacrificeMinion.CanSacrificeMerge(0, 1, 0, 1));
        Assert.IsTrue(SacrificeMinion.CanSacrificeMerge(MAX - 1, 1, MAX - 1, 1));
    }

    [Test]
    public void Sacrifice_DifferentLv_CannotMerge()
    {
        Assert.IsFalse(SacrificeMinion.CanSacrificeMerge(0, 1, 1, 1));
    }

    [Test]
    public void Sacrifice_MaxLv_CannotMerge()
    {
        Assert.IsFalse(SacrificeMinion.CanSacrificeMerge(MAX, 1, MAX, 1));
    }
}

// 슬라임 확률 연속 실패 보정: 연속 4회 실패 후 다음 시도부터 성공 +50%p, 성공하면 초기화
public class MinionChancePityTest
{
    [TestCase(0.3f, 0, 0.3f)]
    [TestCase(0.3f, 3, 0.3f)]   // 3연속 실패까지는 그대로
    [TestCase(0.3f, 4, 0.8f)]   // 4연속 실패 후 +50%p
    [TestCase(0.3f, 6, 0.8f)]   // 성공할 때까지 유지
    [TestCase(0.7f, 4, 1f)]     // 최대 100%
    public void GetChance(float baseChance, int failStreak, float expected)
    {
        Assert.AreEqual(expected, MinionChancePity.GetChance(baseChance, failStreak), 0.0001f);
    }

    [Test]
    public void FailStreak_ResetOnSuccess()
    {
        int streak = 0;
        for (int i = 0; i < 3; i++)
            streak = MinionChancePity.NextFailStreak(streak, false);
        Assert.AreEqual(3, streak);
        Assert.AreEqual(0, MinionChancePity.NextFailStreak(streak, true)); // 3연속 실패 후 성공 → 초기화
        Assert.AreEqual(5, MinionChancePity.NextFailStreak(4, false));
    }

    [Test]
    public void Roll_ZeroOrFullChance_NoStreakChange()
    {
        MinionChancePity pity = new MinionChancePity();
        Assert.IsFalse(pity.Roll("Joker", 0f));
        Assert.IsTrue(pity.Roll("Joker", 1f));
        Assert.AreEqual(0, pity.GetFailStreak("Joker"));
    }
}

public class UserLevelTest
{
    // 최대 레벨(30)이면 경험치를 받지 않음
    [TestCase(0, 10, 10)]
    [TestCase(29, 10, 10)]
    [TestCase(30, 10, 0)]
    [TestCase(31, 10, 0)]
    [TestCase(5, -3, 0)]
    public void GetGainExp(int lv, int exp, int expected)
    {
        Assert.AreEqual(30, UserMyInfoManager.MAX_LEVEL);
        Assert.AreEqual(expected, UserMyInfoManager.GetGainExp(lv, exp));
    }
}

// 슬라임 조합 프리셋: 프리셋 key → 실제 장착 key 정리
public class MinionPresetTest
{
    static readonly string[] Current = { "A", "B", "C", "D", "E" };
    static readonly string[] Owned = { "A", "B", "C", "D", "E", "F", "G" };

    [Test]
    public void ValidPresetUsedAsIs()
    {
        string[] preset = { "F", "G", "A", "B", "C" };
        CollectionAssert.AreEqual(preset, UserMinionManager.ResolvePresetKeys(preset, Current, Owned));
    }

    [Test]
    public void UnownedKeyReplacedByCurrentSlotKey()
    {
        string[] preset = { "X", "G", "A", "B", "C" };
        // 0번 칸 X 미보유 → 현재 0번 칸 A는 이미 사용 → 현재 장착 중 남은 D
        CollectionAssert.AreEqual(new[] { "D", "G", "A", "B", "C" },
            UserMinionManager.ResolvePresetKeys(preset, Current, Owned));
    }

    [Test]
    public void DuplicateAndEmptyKeysReplaced()
    {
        string[] preset = { "F", "F", null, "B", "C" };
        string[] result = UserMinionManager.ResolvePresetKeys(preset, Current, Owned);
        Assert.AreEqual(5, result.Length);
        CollectionAssert.AllItemsAreUnique(result);
        CollectionAssert.AllItemsAreNotNull(result);
        Assert.AreEqual("F", result[0]);
        Assert.AreEqual("A", result[1]); // 현재 1번 칸 B는 3번 칸에서 사용 → 현재 장착 중 남은 A
    }
}

// 엘리트 처치 골드 = (wave 번호 / 5) × 30
public class EliteRewardTest
{
    [Test]
    public void EliteGoldScalesWithWave()
    {
        Assert.AreEqual(30, EliteEnemy.GetEliteRewardGold(4));  // wave 5
        Assert.AreEqual(60, EliteEnemy.GetEliteRewardGold(9));  // wave 10
        Assert.AreEqual(90, EliteEnemy.GetEliteRewardGold(14)); // wave 15
        Assert.AreEqual(30, EliteEnemy.GetEliteRewardGold(0));  // 최소 1단계
    }
}

// 에너지: 게임 시작에 5 소모, 10분마다 1 충전
public class EnergeRuleTest
{
    [Test]
    public void PlayNeedsCostEnerge()
    {
        Assert.AreEqual(5, GameSetting.PLAY_COST_ENERGE);
        Assert.IsTrue(EnergeManager.CanPlay(5));
        Assert.IsFalse(EnergeManager.CanPlay(4));
    }

    [Test]
    public void ChargesOnePerTenMinutes()
    {
        Assert.AreEqual(10, EnergeManager.CHARGE_INTERVAL_MINUTES);
        Assert.AreEqual(0, EnergeManager.GetChargeCount(9.9));
        Assert.AreEqual(1, EnergeManager.GetChargeCount(10));
        Assert.AreEqual(3, EnergeManager.GetChargeCount(35));
        Assert.AreEqual(0, EnergeManager.GetChargeCount(-20)); // 기기 시간을 되돌린 경우
    }
}
