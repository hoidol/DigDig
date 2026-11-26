using NUnit.Framework;

// 로비 보스 처치 연출: 처음 처치한 보스 판정 (LobbyReturnController). (2026-11-26) 보스 = wave 25·50
public class LobbyReturnRuleTest
{
    [Test]
    public void DefeatedBossCount()
    {
        Assert.AreEqual(0, LobbyReturnController.GetDefeatedBossCount(0, false));
        Assert.AreEqual(0, LobbyReturnController.GetDefeatedBossCount(24, false)); // wave 25(보스) 진행 중 사망
        Assert.AreEqual(1, LobbyReturnController.GetDefeatedBossCount(25, false)); // 보스 처치 → wave 26
        Assert.AreEqual(1, LobbyReturnController.GetDefeatedBossCount(49, false)); // wave 50 보스 진행 중
        Assert.AreEqual(2, LobbyReturnController.GetDefeatedBossCount(49, true));  // 클리어
    }

    [Test]
    public void NextBossWave()
    {
        Assert.AreEqual(25, LobbyReturnController.GetNextBossWave(0));
        Assert.AreEqual(50, LobbyReturnController.GetNextBossWave(1));
        Assert.AreEqual(0, LobbyReturnController.GetNextBossWave(2)); // 다 잡음
    }

    [Test]
    public void NewlyDefeatedBossWave()
    {
        Assert.AreEqual(25, LobbyReturnController.GetNewlyDefeatedBossWave(0, 25, false));  // 처음 wave 25 보스
        Assert.AreEqual(0, LobbyReturnController.GetNewlyDefeatedBossWave(1, 30, false));   // 이미 잡은 보스
        Assert.AreEqual(50, LobbyReturnController.GetNewlyDefeatedBossWave(0, 49, true));   // 한 판에 둘 → 늦은 쪽(클리어)
        Assert.AreEqual(50, LobbyReturnController.GetNewlyDefeatedBossWave(1, 49, true));   // 마지막 보스(클리어)
        Assert.AreEqual(0, LobbyReturnController.GetNewlyDefeatedBossWave(0, 24, false));   // 보스 wave에서 사망
    }

    // NextBoss 글씨 = 처치한 보스 +25, 최종 보스면 Clear / 평소 보스 = 다음 보스, 다 잡았으면 최종 보스
    [Test]
    public void WaveLabels()
    {
        Assert.AreEqual("Wave 25", LobbyReturnController.GetWaveLabel(25));
        Assert.AreEqual("Wave 50", LobbyReturnController.GetNextWaveLabel(25));
        Assert.AreEqual("Clear", LobbyReturnController.GetNextWaveLabel(50));
        Assert.AreEqual(25, LobbyReturnController.GetDisplayBossWave(25));
        Assert.AreEqual(50, LobbyReturnController.GetDisplayBossWave(0));
    }
}
