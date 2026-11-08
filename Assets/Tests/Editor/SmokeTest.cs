using NUnit.Framework;

public class SmokeTest
{
    [Test]
    public void MaxMergeLevel_Is4()
    {
        Assert.AreEqual(4, SlimeData.MAX_MERGE_LEVEL);
    }

    [Test]
    public void AttackSpeedPerSecond_Is50()
    {
        Assert.AreEqual(50f, GameSetting.ATTACK_SPEED_PER_SECOND);
    }
}
