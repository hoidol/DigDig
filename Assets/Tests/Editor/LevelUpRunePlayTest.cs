using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-21) 레벨업 능력치 카드 → 룬 카드: 고르면 타일 선택 → 빈 칸 = 새김 / 같은 룬 칸 = 레벨 +1 / 다른 룬 칸 = 팝업(계속 고름)
public class LevelUpRunePlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator RuneCard_TilePick()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        KingSlime king = KingSlime.Instance;
        var setting = KingGrowthSetting.Instance;
        king.abilityInventory.Add(setting.raceAbilities[0]); // 종족은 고른 상태 → 다음 레벨업 = 룬 카드
        var canvas = InGame.LevelUpCanvas.Instance;
        var select = SelectItemCanvas.Instance;
        var atk = ItemManager.Instance.GetItemData("AttackPowerTile");
        var spd = ItemManager.Instance.GetItemData("AttackSpeedTile");
        Tile tile = TileManager.Instance.tiles.First(t => t.IsEmpty && t.CanAddTileItem && t.tileObject == null);
        Tile other = TileManager.Instance.tiles.First(t => t != tile && t.IsEmpty && t.CanAddTileItem && t.tileObject == null);

        // 1) 레벨업 → 카드 3장 = 룬, 공격력 룬 → 빈 칸 = Lv.1
        yield return LevelUpAndOpen(king);
        var shown = canvas.levelUpStatPanels.Where(p => p.gameObject.activeSelf).ToList();
        Assert.AreEqual(3, shown.Count);
        Assert.IsTrue(InGame.LevelUpCanvas.MakeChoiceInput(king, setting).statPool.All(d => d.IsRune), "능력치 카드 대신 룬 카드");
        canvas.BeginRunePick(atk);
        yield return WaitFrames(1);
        Assert.IsTrue(InGame.LevelUpCanvas.IsPickingRune && select.IsRunePicking);
        Assert.IsFalse(canvas.gameObject.activeSelf, "타일 고르는 동안 레벨업 창은 숨김");
        // (2026-11-22) 시간 안 멈춤, 패널 = 룬 설명(Lv.0 아님) + "칸 위 미니언에게" 안내, 안내 문구 유지
        Assert.AreNotEqual(0f, Time.timeScale, "레벨업·타일 선택 중 시간은 안 멈춤");
        StringAssert.Contains(TranslateManager.GetText("Rune_AppliesToUnit"), select.selectedPanel.descText.text);
        Assert.AreNotEqual("Lv.0", select.selectedPanel.descText.text);
        Assert.AreEqual(TranslateManager.GetText("Rune_SelectTile"), select.selectTileGuideText.text);
        MinionFieldController.Instance.tilePickCallback(tile);
        yield return WaitFrames(3);
        var placed = tile.tileItem as TileItem;
        Assert.IsNotNull(placed, "룬이 새겨지지 않음");
        Assert.AreEqual("AttackPowerTile", placed.key);
        Assert.AreEqual(1, placed.upgradeLevel);
        Assert.AreEqual(0, king.pendingLevelUps);
        Assert.IsFalse(InGame.LevelUpCanvas.IsPickingRune);

        // 2) 같은 룬을 같은 칸 → Lv.2 (10% → 20%)
        yield return LevelUpAndOpen(king);
        canvas.BeginRunePick(atk);
        MinionFieldController.Instance.tilePickCallback(tile);
        yield return WaitFrames(2);
        Assert.AreEqual(2, placed.upgradeLevel, "같은 룬이면 레벨 +1");
        StringAssert.Contains("20", placed.GetDescription());

        // 3) 다른 룬을 그 칸 → 팝업, 계속 고르는 중 → 빈 칸에 새김
        yield return LevelUpAndOpen(king);
        canvas.BeginRunePick(spd);
        MinionFieldController.Instance.tilePickCallback(tile);
        yield return WaitFrames(2);
        Assert.IsTrue(YesOrNoCanvas.Instance.gameObject.activeSelf, "다른 룬 팝업이 안 뜸");
        Assert.AreEqual(TranslateManager.GetText("Rune_OtherInstalled"), YesOrNoCanvas.Instance.bodyText.text);
        Assert.IsTrue(select.IsRunePicking, "팝업 뒤에도 계속 고름");
        Assert.AreEqual(2, placed.upgradeLevel, "다른 룬은 레벨 그대로");
        YesOrNoCanvas.Instance.CloseCanvas();
        MinionFieldController.Instance.tilePickCallback(other);
        yield return WaitFrames(3);
        Assert.AreEqual("AttackSpeedTile", (other.tileItem as TileItem)?.key);
        Assert.AreEqual(0, king.pendingLevelUps);
    }

    static IEnumerator LevelUpAndOpen(KingSlime king)
    {
        king.AddExp(king.MaxExp);
        yield return WaitFrames(1);
        InGame.LevelUpCanvas.OpenPending();
        yield return WaitFrames(2);
        Assert.IsTrue(InGame.LevelUpCanvas.Instance.gameObject.activeSelf, "레벨업 창이 안 열림");
    }
}
