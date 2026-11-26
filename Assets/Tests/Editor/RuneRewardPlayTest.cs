using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-23) 엘리트·보스 처치 룬 보상: 보상 적립 → 브레이크타임 시작에 룬 3장 선택 창 → 칸 선택 → 새김
// 반드시 골라야 함(닫으면 룬 3장 화면으로), 고르는 동안 Next 숨김·레벨업 창 대기, 보상 2개면 연속으로 열림
public class RuneRewardPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator RuneReward_NextBreakTime()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        KingSlime king = KingSlime.Instance;
        var select = SelectRuneCanvas.Instance;
        Assert.IsTrue(GameManager.Instance.isBreakTime);

        // 1) 엘리트·보스 처치 2번 → 적립만 (창은 다음 브레이크타임 시작에)
        RuneRewardSystem.AddReward(king.transform.position);
        RuneRewardSystem.AddReward(king.transform.position);
        Assert.AreEqual(2, king.pendingRuneRewards);
        Assert.IsFalse(RuneRewardSystem.IsOpen);
        Assert.AreEqual(2, RuneRewardChest.Count, "처치 자리에 보상 상자 2개");

        // 2) 브레이크타임 시작 → 룬 3장
        GameEventBus.Publish(new BreakTimeStartEvent(GameManager.Instance.waveIdx));
        yield return WaitFrames(2);
        Assert.IsTrue(RuneRewardSystem.IsOpen, "룬 보상 창이 안 열림");
        Assert.AreEqual(1, RuneRewardChest.Count, "창이 열리면 필드 상자 하나가 사라짐");
        Assert.IsTrue(select.rewardChestRoot != null && select.rewardChestRoot.gameObject.activeInHierarchy, "창에 보상 상자 표시");
        Assert.AreEqual(select.rewardChestSprite, select.rewardChestImage.sprite);
        Assert.IsNotNull(select.rewardChestSprite, "씬에 상자 그림 연결");
        Assert.AreEqual(SelectRuneStep.SelectRune, select.step);
        var shown = select.selectRunePanels.Where(p => p.gameObject.activeSelf).ToList();
        Assert.AreEqual(RuneRewardSystem.CHOICE_COUNT, shown.Count, "룬 3장");
        var nextButton = Object.FindFirstObjectByType<MinionOfferSystem>().nextButton;
        Assert.IsFalse(nextButton.gameObject.activeSelf, "룬을 고르는 동안 Next 숨김");

        // 3) 레벨업은 룬 보상이 끝날 때까지 대기
        king.AddExp(king.MaxExp);
        yield return WaitFrames(2);
        Assert.IsFalse(InGame.LevelUpCanvas.Instance.gameObject.activeSelf, "룬 보상 중엔 레벨업 창 대기");

        // 4) 닫으려 해도 룬 3장 화면으로 (반드시 고름)
        select.CloseCanvas();
        yield return WaitFrames(1);
        Assert.IsTrue(select.gameObject.activeSelf && RuneRewardSystem.IsOpen, "룬 보상은 닫히지 않아야 함");

        // 5) 룬 고름 → 칸 선택 → 새김 → 두 번째 보상 창이 바로 열림
        RuneData rune = select.shownRunesForTest.First(d => !(d.runePrefab is PioneerTileRune));
        select.Selected(rune);
        Assert.AreEqual(SelectRuneStep.SelectTile, select.step);
        select.CloseCanvas(); // 칸 선택 취소 → 룬 3장으로
        Assert.AreEqual(SelectRuneStep.SelectRune, select.step, "칸 선택 취소 → 룬 선택으로");
        select.Selected(rune);
        Tile tile = TileManager.Instance.tiles.First(t => t.IsEmpty && t.CanAddTileRune && t.tileObject == null);
        MinionFieldController.Instance.tilePickCallback(tile);
        yield return WaitFrames(3);
        Assert.AreEqual(rune.key, (tile.tileRune as TileRune)?.key, "룬이 새겨지지 않음");
        Assert.AreEqual(1, king.pendingRuneRewards);
        Assert.IsTrue(RuneRewardSystem.IsOpen, "보상이 남았으면 연속으로 열림");

        // 6) 두 번째 보상 → 끝나면 밀린 레벨업 창
        RuneData rune2 = select.shownRunesForTest.First(d => !(d.runePrefab is PioneerTileRune));
        select.Selected(rune2);
        Tile tile2 = TileManager.Instance.tiles.First(t => t != tile && t.IsEmpty && t.CanAddTileRune && t.tileObject == null);
        MinionFieldController.Instance.tilePickCallback(tile2);
        yield return WaitFrames(3);
        Assert.AreEqual(0, king.pendingRuneRewards);
        Assert.IsFalse(RuneRewardSystem.IsOpen);
        Assert.IsFalse(select.gameObject.activeSelf);
        Assert.IsTrue(InGame.LevelUpCanvas.Instance.gameObject.activeSelf, "룬 보상이 끝나면 밀린 레벨업 창");
    }
}
