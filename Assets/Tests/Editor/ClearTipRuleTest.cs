using System.Linq;
using NUnit.Framework;

// (2026-11-26) 클리어 팁 규칙: 공백 정리·40자, 금칙어 *** (대소문자 무시), 서버 금칙어 문자열 파싱, 띠지 순서(pinned 먼저 → 최신)
public class ClearTipRuleTest
{
    [Test]
    public void Normalize_TrimsAndLimits()
    {
        Assert.AreEqual("드디어 깼다", ClearTipManager.Normalize("  드디어\n  깼다  "));
        Assert.AreEqual("", ClearTipManager.Normalize("   "));
        Assert.AreEqual(ClearTipManager.MAX_LENGTH, ClearTipManager.Normalize(new string('가', 100)).Length);
    }

    [Test]
    public void Mask_BannedWords()
    {
        Assert.AreEqual("이 ** 보스", ClearTipManager.Mask("이 시발 보스", new[] { "시발" }));
        Assert.AreEqual("**** you", ClearTipManager.Mask("FUCK you", new[] { "fuck" }));
        Assert.AreEqual("평범한 팁", ClearTipManager.Mask("평범한 팁", new[] { "시발" }));
        Assert.AreEqual("**ㅋ", ClearTipManager.MaskNow("병신ㅋ"), "코드 기본 금칙어");
    }

    [Test]
    public void ParseBannedWords_CommaSeparated()
    {
        CollectionAssert.AreEqual(new[] { "바보", "멍청이" }, ClearTipManager.ParseBannedWords(" 바보, 멍청이 ,,"));
        Assert.IsEmpty(ClearTipManager.ParseBannedWords(null));
    }

    // (2026-11-26) 내가 남긴 팁 = 2번째 (같은 uid 서버 항목은 빼고, 짧으면 맨 끝)
    [Test]
    public void PlaceMine_Second()
    {
        var mine = new ClearTip { uid = "me", text = "내 팁" };
        var list = new System.Collections.Generic.List<ClearTip>
        {
            new ClearTip { uid = "me", text = "서버의 내 팁" },
            new ClearTip { uid = "a", text = "A" },
            new ClearTip { uid = "b", text = "B" },
        };
        CollectionAssert.AreEqual(new[] { "A", "내 팁", "B" }, ClearTipManager.PlaceMine(list, mine).Select(t => t.text).ToArray());
        CollectionAssert.AreEqual(new[] { "내 팁" }, ClearTipManager.PlaceMine(new System.Collections.Generic.List<ClearTip>(), mine).Select(t => t.text).ToArray());
        Assert.AreEqual(3, ClearTipManager.PlaceMine(list, null).Count);
    }

    [Test]
    public void Order_PinnedFirstThenLatest()
    {
        var tips = new[]
        {
            new ClearTip { uid = "a", text = "old", updatedAt = 1 },
            new ClearTip { uid = "b", text = "new", updatedAt = 3 },
            new ClearTip { uid = "admin", text = "공지", updatedAt = 0, pinned = true },
            new ClearTip { uid = "c", text = "  ", updatedAt = 5 },
        };
        CollectionAssert.AreEqual(new[] { "admin", "b", "a" }, ClearTipManager.Order(tips).Select(t => t.uid).ToArray());
    }
}
