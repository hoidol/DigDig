using System.Collections;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 아이템 뽑기 버튼 흐름 (CLAUDE.md "타일 아이템 > 아이템 획득")
// 코인 지불 → Draw(확률) → SelectItem(겹치지 않는 3개) → SelectTile(눌러서 설치)
public class DrawItemButtonPlayTest : InGamePlayTestBase
{
    const int TEST_COIN = 1000;
    const float DRAW_WAIT = 2f; // drawDuration(1초, timeScale 무시) + 여유. 실시간(초)

    static readonly FieldInfo PANEL_ITEM_DATA = typeof(SelectItemPanel).GetField("itemData", BindingFlags.NonPublic | BindingFlags.Instance);

    DrawItemButton drawButton;
    SelectItemCanvas canvas;

    IEnumerator Setup()
    {
        yield return WaitGameStart();
        drawButton = Object.FindFirstObjectByType<DrawItemButton>(FindObjectsInactive.Include);
        Assert.IsNotNull(drawButton, "씬에 DrawItemButton 없음");
        Assert.IsTrue(drawButton.gameObject.activeInHierarchy, "DrawItemButton이 꺼져 있음");
        Assert.IsTrue(drawButton.button.interactable, "DrawItemButton을 누를 수 없음");
        canvas = SelectItemCanvas.Instance;
        Assert.IsNotNull(canvas, "SelectItemCanvas 없음");
    }

    static void SetCoin(int coin) => KingSlime.Instance.AddCoin(coin - KingSlime.Instance.coin);
    static int Price(int drawCount) => GameSetting.INIT_DRAW_ITEM_PRICE + drawCount * GameSetting.INCREASE_DRAW_ITEM_PRICE;
    void Click() => drawButton.button.onClick.Invoke(); // 실제 Button 이벤트로 누름

    // 판정 Random.Range(0, 100) < chance 가 원하는 결과가 나오는 시드
    static int FindSeed(int chance, bool success)
    {
        for (int seed = 0; ; seed++)
        {
            Random.InitState(seed);
            if ((Random.Range(0, 100) < chance) == success)
                return seed;
        }
    }

    [UnityTest]
    public IEnumerator Success_SelectItem_InstallOnTile()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return Setup();

        GameManager.Instance.drawItemCount = 0;
        SetCoin(TEST_COIN);
        Assert.AreEqual(100, DrawItemButton.GetDrawChance(), "처음 확률은 100%");

        Click();
        Assert.AreEqual(TEST_COIN - Price(0), KingSlime.Instance.coin, "뽑기 가격만큼 코인이 빠지지 않음");
        Assert.IsTrue(canvas.gameObject.activeSelf, "SelectItemCanvas가 열리지 않음");
        Assert.AreEqual(SelectItemStep.Draw, canvas.step);

        yield return WaitRealtime(DRAW_WAIT);
        Assert.AreEqual(SelectItemStep.SelectItem, canvas.step, "100%인데 아이템 선택 단계로 안 넘어감");
        Assert.AreEqual(1, GameManager.Instance.drawItemCount, "성공했는데 뽑기 횟수가 안 오름");
        Assert.AreEqual(95, DrawItemButton.GetDrawChance(), "성공 후 확률 -5%");

        var panels = canvas.selectItemPanels.Where(p => p.gameObject.activeSelf).ToList();
        var shown = panels.Select(p => (ItemData)PANEL_ITEM_DATA.GetValue(p)).ToList();
        Assert.That(panels.Count, Is.InRange(1, 3), "선택지 수");
        Assert.AreEqual(shown.Count, shown.Distinct().Count(), "선택지가 겹침");
        Assert.IsTrue(shown.All(d => d.itemPrefab is TileItem), "타일 아이템이 아닌 선택지");

        // 타일 선택이 필요한 아이템 선택 → 타일 선택 단계
        int idx = shown.FindIndex(d => ((TileItem)d.itemPrefab).NeedTile);
        Assert.GreaterOrEqual(idx, 0, "타일에 설치하는 선택지가 없음");
        ItemData picked = shown[idx];
        panels[idx].OnClickedButton();
        Assert.AreEqual(SelectItemStep.SelectTile, canvas.step, "타일 선택 단계로 안 넘어감");
        Assert.IsNotNull(SlimeFieldController.Instance.tilePickCallback, "타일 선택 모드가 아님");

        // 이미 아이템이 있는 타일은 거부
        Tile occupied = GetFreeTile();
        KingSlime.Instance.AddItem("AttackPowerTile", occupied).Forget();
        yield return WaitFrames(2);
        object before = occupied.tileItem;
        SlimeFieldController.Instance.tilePickCallback(occupied);
        Assert.AreSame(before, occupied.tileItem, "이미 아이템이 있는 타일에 설치됨");
        Assert.AreEqual(SelectItemStep.SelectTile, canvas.step, "거부 후에도 타일 선택 단계여야 함");

        // 빈 타일 선택 → 설치되고 캔버스 닫힘
        Tile target = GetFreeTile();
        SlimeFieldController.Instance.tilePickCallback(target);
        yield return WaitFrames(2);
        Assert.IsInstanceOf<TileItem>(target.tileItem, "고른 타일에 설치되지 않음");
        Assert.AreEqual(picked.key, ((TileItem)target.tileItem).key, "고른 아이템과 다른 아이템이 설치됨");
        Assert.IsFalse(canvas.gameObject.activeSelf, "설치 후 캔버스가 닫히지 않음");
        Assert.IsNull(SlimeFieldController.Instance.tilePickCallback, "설치 후에도 타일 선택 모드");

        // 다음 가격 +증가분
        SetCoin(TEST_COIN);
        Click();
        Assert.AreEqual(TEST_COIN - Price(1), KingSlime.Instance.coin, "두 번째 뽑기 가격");
        AssertNoErrors();
    }

    [UnityTest]
    public IEnumerator Fail_OnlyCoinSpent()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return Setup();

        GameManager.Instance.drawItemCount = 10; // 확률 50%
        int chance = DrawItemButton.GetDrawChance();
        SetCoin(TEST_COIN);
        Random.InitState(FindSeed(chance, false)); // 판정은 누르는 순간 동기로 한 번
        Click();
        Assert.AreEqual(TEST_COIN - Price(10), KingSlime.Instance.coin);

        yield return WaitRealtime(DRAW_WAIT);
        Assert.IsFalse(canvas.gameObject.activeSelf, "실패했는데 캔버스가 닫히지 않음");
        Assert.AreEqual(10, GameManager.Instance.drawItemCount, "실패하면 뽑기 횟수(가격·확률) 그대로");
        Assert.AreEqual(chance, DrawItemButton.GetDrawChance());
        AssertNoErrors();
    }

    [UnityTest]
    public IEnumerator NotEnoughCoin_DoesNothing()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return Setup();

        GameManager.Instance.drawItemCount = 0;
        SetCoin(Price(0) - 1);
        Click();
        yield return WaitFrames(2);
        Assert.AreEqual(Price(0) - 1, KingSlime.Instance.coin, "코인이 모자란데 빠짐");
        Assert.IsFalse(canvas.gameObject.activeSelf, "코인이 모자란데 캔버스가 열림");
        AssertNoErrors();
    }
}
