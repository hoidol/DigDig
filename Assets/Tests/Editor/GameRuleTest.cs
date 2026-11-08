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

    // 4 wave마다 Ethereal → Armored 번갈아, 나머지는 Normal
    [TestCase(0, DefenceType.Normal)]
    [TestCase(2, DefenceType.Normal)]
    [TestCase(3, DefenceType.Ethereal)]  // 4번째
    [TestCase(4, DefenceType.Normal)]
    [TestCase(7, DefenceType.Armored)]   // 8번째
    [TestCase(11, DefenceType.Ethereal)] // 12번째
    [TestCase(15, DefenceType.Armored)]  // 16번째
    [TestCase(19, DefenceType.Ethereal)] // 20번째 (보스 wave, 보스 자체는 항상 Normal)
    public void GetWaveDefenceType(int waveIdx, DefenceType expected)
    {
        Assert.AreEqual(expected, GameManager.GetWaveDefenceType(waveIdx));
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
    const int MAX = SlimeData.MAX_MERGE_LEVEL;

    [Test]
    public void SameKeySameLv_CanMerge()
    {
        Assert.IsTrue(Slime.CanMerge("Ice", 0, 1, "Ice", 0, 1));
        Assert.IsTrue(Slime.CanMerge("Ice", MAX - 1, 1, "Ice", MAX - 1, 1));
    }

    [Test]
    public void DifferentKey_CannotMerge()
    {
        Assert.IsFalse(Slime.CanMerge("Ice", 0, 1, "Flame", 0, 1));
    }

    [Test]
    public void DifferentLv_CannotMerge()
    {
        Assert.IsFalse(Slime.CanMerge("Ice", 0, 1, "Ice", 1, 1));
    }

    [Test]
    public void MaxLv_CannotMerge()
    {
        Assert.IsFalse(Slime.CanMerge("Ice", MAX, 1, "Ice", MAX, 1));
    }

    [Test]
    public void Sacrifice_AnyKeySameLv_CanMerge()
    {
        Assert.IsTrue(SacrificeSlime.CanSacrificeMerge(0, 1, 0, 1));
        Assert.IsTrue(SacrificeSlime.CanSacrificeMerge(MAX - 1, 1, MAX - 1, 1));
    }

    [Test]
    public void Sacrifice_DifferentLv_CannotMerge()
    {
        Assert.IsFalse(SacrificeSlime.CanSacrificeMerge(0, 1, 1, 1));
    }

    [Test]
    public void Sacrifice_MaxLv_CannotMerge()
    {
        Assert.IsFalse(SacrificeSlime.CanSacrificeMerge(MAX, 1, MAX, 1));
    }
}
