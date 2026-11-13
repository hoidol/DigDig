using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 진화 타일 뽑기 (CLAUDE.md "타일 아이템"): 진화 안 한 최대 레벨 슬라임 수 > 진화 타일 수일 때만,
// 뽑기마다 40%로 선택지 3개에 확정 등장. 같으면 안 나옴
public class EvolutionTileDrawPlayTest : InGamePlayTestBase
{
    const string KEY = "EvolutionTile";
    const int DRAWS = 300;

    [Test]
    public void Rule_CountLimit()
    {
        Assert.IsTrue(EvolutionTileItem.CanDrawFor(5, 4), "5마리 / 4개 → 등장 가능");
        Assert.IsFalse(EvolutionTileItem.CanDrawFor(5, 5), "5마리 / 5개 → 안 나옴");
        Assert.IsFalse(EvolutionTileItem.CanDrawFor(5, 6), "5마리 / 6개 → 안 나옴");
        Assert.IsFalse(EvolutionTileItem.CanDrawFor(0, 0), "최대 레벨 슬라임 없음");
        Assert.AreEqual(40f, EvolutionTileItem.DRAW_CHANCE);
    }

    [UnityTest]
    public IEnumerator Draw_ChanceAndLimit()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Draw_ChanceAndLimitBody();
    }

    IEnumerator Draw_ChanceAndLimitBody()
    {
        Assert.IsNotNull(ItemManager.Instance.GetItemData(KEY), "진화 타일 ItemData 없음");
        MinionFieldController.Instance.minionMaxCount = 99;

        // 최대 레벨 슬라임 0 → 안 나옴
        Assert.AreEqual(0, CountAppear(), "최대 레벨 슬라임이 없는데 등장");

        // 최대 레벨 슬라임 2마리, 진화 타일 0 → 약 40%
        SpawnMinion("Ice", GetFreeTile(), MinionData.MAX_MERGE_LEVEL);
        SpawnMinion("Ice", GetFreeTile(), MinionData.MAX_MERGE_LEVEL);
        yield return WaitFrames(2);
        Assert.AreEqual(2, EvolutionTileItem.EvolvableMinionCount);
        int appear = CountAppear();
        Assert.That(appear, Is.InRange(DRAWS * 0.3f, DRAWS * 0.5f), $"등장 {appear}/{DRAWS} (기대 약 40%)");

        // 진화 타일 1개 (빈 칸에 설치, 슬라임 2마리 > 1개) → 여전히 등장
        KingSlime.Instance.AddItem(KEY, GetFreeTile()).Forget();
        yield return WaitFrames(2);
        Assert.AreEqual(1, EvolutionTileItem.InstalledCount);
        Assert.Greater(CountAppear(), 0, "슬라임 2 > 타일 1인데 안 나옴 (보유 상한에 막힘?)");

        // 진화 타일 2개 = 슬라임 2마리 → 안 나옴
        KingSlime.Instance.AddItem(KEY, GetFreeTile()).Forget();
        yield return WaitFrames(2);
        Assert.AreEqual(2, EvolutionTileItem.InstalledCount);
        Assert.AreEqual(0, CountAppear(), "슬라임 수 = 타일 수인데 등장");

        AssertNoErrors();
    }

    // 뽑기 DRAWS번 중 선택지 3개에 진화 타일이 든 횟수
    static int CountAppear()
    {
        int n = 0;
        for (int i = 0; i < DRAWS; i++)
            if (ItemManager.Instance.GetDrawTileItems(SelectItemCanvas.SELECT_ITEM_COUNT).Any(d => d.key == KEY))
                n++;
        return n;
    }
}
