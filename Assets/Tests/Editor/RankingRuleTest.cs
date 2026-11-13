using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

// 랭킹 규칙 (CLAUDE.md "랭킹"): 순위 매기기, 목록 밖 표시, 닉네임, wave 글씨
public class RankingRuleTest
{
    static RankData Data(string uid, int wave, long updatedAt) => new RankData { uid = uid, nickname = uid, wave = wave, updatedAt = updatedAt };

    // wave 내림차순, 같은 wave면 먼저 달성한 사람이 위
    [Test]
    public void Order_WaveThenEarlier()
    {
        var result = RankingManager.BuildResult(DifficultyType.Normal, new[]
        {
            Data("late30", 30, 500), Data("w10", 10, 1), Data("early30", 30, 100), Data("w50", 50, 900),
        }, "early30", 0f);
        Assert.AreEqual(new[] { "w50", "early30", "late30", "w10" }, result.ranks.Select(r => r.uid).ToArray());
        Assert.AreEqual(new[] { 1, 2, 3, 4 }, result.ranks.Select(r => r.num).ToArray());
        Assert.AreEqual("early30", result.my.uid);
        Assert.AreEqual(2, result.my.num);
        Assert.AreEqual(DifficultyType.Normal, result.difficulty);
    }

    // 상위 100명만, 내가 그 밖이면 my = null
    [Test]
    public void Top100_MyOutside()
    {
        var datas = new List<RankData>();
        for (int i = 0; i < 130; i++)
            datas.Add(Data($"u{i}", 1000 - i, i));
        datas.Add(Data("me", 5, 0));
        var result = RankingManager.BuildResult(DifficultyType.Hard, datas, "me", 0f);
        Assert.AreEqual(RankingManager.TOP_COUNT, result.ranks.Count);
        Assert.AreEqual("u0", result.ranks[0].uid);
        Assert.AreEqual(100, result.ranks[99].num);
        Assert.IsNull(result.my, "목록 밖이면 my 없음");
    }

    [Test]
    public void Empty()
    {
        var result = RankingManager.BuildResult(DifficultyType.Hell, new RankData[0], "me", 0f);
        Assert.IsEmpty(result.ranks);
        Assert.IsNull(result.my);
    }

    // 목록 밖: 기록 있으면 "100+", 없으면 "-"
    [Test]
    public void OutOfRankLabel()
    {
        Assert.AreEqual("100+", RankingManager.GetOutOfRankLabel(12));
        Assert.AreEqual("-", RankingManager.GetOutOfRankLabel(0));
    }

    // 닉네임이 비었거나 기본값(GUID)이면 Guest#uid앞4자리
    [Test]
    public void Nickname()
    {
        Assert.AreEqual("슬라임왕", RankingManager.GetDisplayNickname("슬라임왕", "abcdef123"));
        Assert.AreEqual("Guest#abcd", RankingManager.GetDisplayNickname(System.Guid.NewGuid().ToString(), "abcdef123"));
        Assert.AreEqual("Guest#abcd", RankingManager.GetDisplayNickname("", "abcdef123"));
        Assert.AreEqual("Guest#ab", RankingManager.GetDisplayNickname(null, "ab"));
        Assert.AreEqual("Guest#????", RankingManager.GetDisplayNickname(null, null));
    }

    // 머리글에 WAVE가 있으므로 숫자만, 기록 없음은 "-"
    [Test]
    public void WaveText()
    {
        Assert.AreEqual("37", RankingPanel.GetWaveText(37));
        Assert.AreEqual("-", RankingPanel.GetWaveText(0));
    }
}
