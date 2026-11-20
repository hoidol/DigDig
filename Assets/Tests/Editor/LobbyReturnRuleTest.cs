using NUnit.Framework;

// 로비 보스 처치 연출: 처음 처치한 보스 판정 (LobbyReturnController)
public class LobbyReturnRuleTest
{
    [Test]
    public void DefeatedBossCount()
    {
        Assert.AreEqual(0, LobbyReturnController.GetDefeatedBossCount(0, false));
        Assert.AreEqual(0, LobbyReturnController.GetDefeatedBossCount(19, false)); // wave 20(보스) 진행 중 사망
        Assert.AreEqual(1, LobbyReturnController.GetDefeatedBossCount(20, false)); // 보스 처치 → wave 21
        Assert.AreEqual(2, LobbyReturnController.GetDefeatedBossCount(59, false)); // wave 60 보스 진행 중
        Assert.AreEqual(3, LobbyReturnController.GetDefeatedBossCount(59, true));  // 클리어
    }

    [Test]
    public void NextBossWave()
    {
        Assert.AreEqual(20, LobbyReturnController.GetNextBossWave(0));
        Assert.AreEqual(40, LobbyReturnController.GetNextBossWave(1));
        Assert.AreEqual(60, LobbyReturnController.GetNextBossWave(2));
        Assert.AreEqual(0, LobbyReturnController.GetNextBossWave(3)); // 다 잡음
    }

    [Test]
    public void NewlyDefeatedBossWave()
    {
        Assert.AreEqual(20, LobbyReturnController.GetNewlyDefeatedBossWave(0, 20, false));  // 처음 wave 20 보스
        Assert.AreEqual(0, LobbyReturnController.GetNewlyDefeatedBossWave(1, 25, false));   // 이미 잡은 보스
        Assert.AreEqual(40, LobbyReturnController.GetNewlyDefeatedBossWave(0, 45, false));  // 한 판에 둘 → 늦은 쪽
        Assert.AreEqual(60, LobbyReturnController.GetNewlyDefeatedBossWave(2, 59, true));   // 마지막 보스(클리어)
        Assert.AreEqual(0, LobbyReturnController.GetNewlyDefeatedBossWave(0, 19, false));   // 보스 wave에서 사망
    }

    // NextBoss 글씨 = 처치한 보스 +20, 최종 보스면 Clear / 평소 보스 = 다음 보스, 다 잡았으면 최종 보스
    [Test]
    public void WaveLabels()
    {
        Assert.AreEqual("Wave 20", LobbyReturnController.GetWaveLabel(20));
        Assert.AreEqual("Wave 40", LobbyReturnController.GetNextWaveLabel(20));
        Assert.AreEqual("Clear", LobbyReturnController.GetNextWaveLabel(60));
        Assert.AreEqual(40, LobbyReturnController.GetDisplayBossWave(40));
        Assert.AreEqual(60, LobbyReturnController.GetDisplayBossWave(0));
    }
}
