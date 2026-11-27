using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 룬 칸 규칙 + 레벨업 공용 도우미(PickMinionEnhanceCard)
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

    // (2026-11-23) 룬은 레벨업에서 빠지고 엘리트·보스 보상(룬 3장 → 칸 선택)으로. 칸 규칙은 그대로:
    // 빈 칸 = 새김 / 같은 룬 칸 = 레벨 +1 / 다른 룬 칸 = 팝업(계속 고름)
    IEnumerator Body()
    {
        Slime slime = Slime.Instance;
        var select = SelectRuneCanvas.Instance;
        var atk = RuneManager.Instance.GetRuneData("AttackPowerTile");
        var spd = RuneManager.Instance.GetRuneData("AttackSpeedTile");
        Tile tile = TileManager.Instance.tiles.First(t => t.IsEmpty && t.CanAddTileRune && t.tileObject == null);
        Tile other = TileManager.Instance.tiles.First(t => t != tile && t.IsEmpty && t.CanAddTileRune && t.tileObject == null);
        int done = 0;
        System.Collections.Generic.List<RuneData> runes = new System.Collections.Generic.List<RuneData> { atk, spd };

        // 1) 레벨업 카드엔 룬이 없음 (용사를 고른 뒤 Lv.8 = 단계 카드 없음)
        Assert.IsEmpty(LevelUpChoiceRule.GetChoices(new LevelUpChoiceRule.Input { level = 8, heroLevel = 3, heroSkillCount = 3, boostCount = 4 }, new System.Random(1)));

        // 2) 공격력 룬 → 빈 칸 = Lv.1, 시간은 안 멈춤, 안내 문구
        select.OpenRuneReward(runes, () => done++);
        select.Selected(atk);
        yield return WaitFrames(1);
        Assert.AreNotEqual(0f, Time.timeScale, "룬 선택 중 시간은 안 멈춤");
        StringAssert.Contains(TranslateManager.GetText("Rune_AppliesToUnit"), select.selectedPanel.descText.text);
        Assert.AreEqual(TranslateManager.GetText("Rune_SelectTile"), select.selectTileGuideText.text);
        MinionFieldController.Instance.tilePickCallback(tile);
        yield return WaitFrames(3);
        var placed = tile.tileRune as TileRune;
        Assert.IsNotNull(placed, "룬이 새겨지지 않음");
        Assert.AreEqual("AttackPowerTile", placed.key);
        Assert.AreEqual(1, placed.upgradeLevel);
        Assert.AreEqual(1, done);

        // 3) 같은 룬을 같은 칸 → Lv.2 (수치는 RuneData.csv Values, 배율형 → % 증가량)
        select.OpenRuneReward(runes, () => done++);
        select.Selected(atk);
        MinionFieldController.Instance.tilePickCallback(tile);
        yield return WaitFrames(2);
        Assert.AreEqual(2, placed.upgradeLevel, "같은 룬이면 레벨 +1");
        string lv2Percent = Mathf.RoundToInt((atk.GetLevelValue(2) - 1f) * 100f).ToString();
        StringAssert.Contains(lv2Percent, placed.GetDescription());

        // 4) 다른 룬을 그 칸 → 팝업, 계속 고르는 중 → 빈 칸에 새김
        select.OpenRuneReward(runes, () => done++);
        select.Selected(spd);
        MinionFieldController.Instance.tilePickCallback(tile);
        yield return WaitFrames(2);
        Assert.IsTrue(YesOrNoCanvas.Instance.gameObject.activeSelf, "다른 룬 팝업이 안 뜸");
        Assert.AreEqual(TranslateManager.GetText("Rune_OtherInstalled"), YesOrNoCanvas.Instance.bodyText.text);
        Assert.IsTrue(select.IsRuneRewardOpen, "팝업 뒤에도 계속 고름");
        Assert.AreEqual(2, placed.upgradeLevel, "다른 룬은 레벨 그대로");
        YesOrNoCanvas.Instance.CloseCanvas();
        MinionFieldController.Instance.tilePickCallback(other);
        yield return WaitFrames(3);
        Assert.AreEqual("AttackSpeedTile", (other.tileRune as TileRune)?.key);
        Assert.AreEqual(3, done);
    }

    static IEnumerator LevelUpAndOpen(Slime slime)
    {
        slime.AddExp(slime.MaxExp);
        yield return WaitFrames(1);
        InGame.LevelUpCanvas.OpenPending();
        yield return WaitFrames(2);
        Assert.IsTrue(InGame.LevelUpCanvas.Instance.gameObject.activeSelf, "레벨업 창이 안 열림");
    }

    // (2026-11-25) 레벨업 마지막 화면 = 미니언 강화 카드 (단계 카드 다음) → 하나 고르면 그 종·스탯 레벨 +1, 레벨업 소모
    public static IEnumerator PickMinionEnhanceCard()
    {
        var canvas = InGame.LevelUpCanvas.Instance;
        Assert.IsTrue(canvas.IsStatStep, "단계 카드 다음은 미니언 강화 카드");
        int pending = Slime.Instance.pendingLevelUps;
        var shown = canvas.levelUpStatPanels.Where(p => p.gameObject.activeSelf).ToList();
        Assert.AreEqual(3, shown.Count, "미니언 강화 카드 3장");
        var datas = shown.Select(p => p.Data).ToList();
        Assert.IsTrue(datas.All(d => d != null && d.IsMinionEnhance), "미니언 강화 카드가 아님");
        Assert.AreEqual(3, datas.Select(d => (d.minionKey, d.minionStat)).Distinct().Count(), "같은 조합 카드가 겹침");
        AbilityData pick = datas[0];
        int before = MinionEnhanceSystem.GetLevel(pick.minionKey, pick.minionStat);
        shown[0].OnClickedSelect();
        yield return WaitFrames(2);
        Assert.AreEqual(before + pick.minionAmount, MinionEnhanceSystem.GetLevel(pick.minionKey, pick.minionStat), "미니언 강화 레벨 +1 (행운 카드 +2)");
        Assert.AreEqual(pending - 1, Slime.Instance.pendingLevelUps, "미니언 강화를 고르면 레벨업 소모");
    }
}
