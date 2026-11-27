using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-26) 브레이크타임 룬 강화: 룬 칸을 누르면 강화 버튼 → 주사위 코인 3개로 레벨 +1 (확정), 최대 레벨·wave 중엔 버튼 숨김
public class RuneUpgradePlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator Upgrade_InBreakTime_CostsDiceCoin()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        Slime slime = Slime.Instance;
        Assert.IsTrue(GameManager.Instance.isBreakTime, "게임 시작 = 브레이크타임");
        Tile tile = GetFreeTile();
        slime.AddRune("AttackPowerTile", tile).Forget();
        yield return WaitFrames(5);
        TileRune rune = tile.tileRune as TileRune;
        Assert.IsNotNull(rune, "룬 설치 안 됨");
        Assert.AreEqual(1, rune.upgradeLevel);

        var panel = Object.FindFirstObjectByType<TileRuneInfoPanel>(FindObjectsInactive.Include);
        Assert.IsNotNull(panel);
        panel.SetTile(tile);
        yield return WaitFrames(1);
        Assert.IsTrue(panel.upgradeButton != null && panel.upgradeButton.gameObject.activeSelf, "브레이크타임 = 강화 버튼");
        StringAssert.Contains("Lv.1", panel.lvText.text);

        // 코인 부족 → 강화 안 됨
        slime.AddDiceCoin(-slime.diceCoin + 2);
        panel.upgradeButton.onClick.Invoke();
        Assert.AreEqual(1, rune.upgradeLevel, "코인 2개로는 강화 안 됨");
        Assert.AreEqual(2, slime.diceCoin);

        // 3개 → Lv.2, 다시 3개 → Lv.3 (최대) → 버튼 숨김
        slime.AddDiceCoin(GameSetting.RUNE_UPGRADE_DICE_COIN * 2 - 2);
        panel.upgradeButton.onClick.Invoke();
        Assert.AreEqual(2, rune.upgradeLevel, "강화 Lv.2");
        Assert.AreEqual(GameSetting.RUNE_UPGRADE_DICE_COIN, slime.diceCoin, "주사위 코인 3개 소모");
        StringAssert.Contains("Lv.2", panel.lvText.text);
        panel.upgradeButton.onClick.Invoke();
        Assert.AreEqual(rune.runeData.MaxLevel, rune.upgradeLevel, "최대 레벨");
        Assert.AreEqual(0, slime.diceCoin);
        Assert.IsFalse(panel.upgradeButton.gameObject.activeSelf, "최대 레벨이면 버튼 숨김");

        // wave 중엔 강화 버튼 없음
        Tile tile2 = GetFreeTile(tile);
        slime.AddRune("AttackSpeedTile", tile2).Forget();
        yield return WaitFrames(5);
        GameManager.Instance.EndBreakTime();
        yield return WaitFrames(2);
        panel.SetTile(tile2);
        yield return WaitFrames(1);
        Assert.IsFalse(panel.upgradeButton.gameObject.activeSelf, "wave 중엔 강화 버튼 숨김");
    }
}
