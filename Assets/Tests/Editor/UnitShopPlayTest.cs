using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-26) 상점 [용사]·[미니언] 판매 칸: 다이아 상품 줄 모양 복사, 용사 = HeroData 전부 / 미니언 = 레벨 해금 미니언
// 보유 = "보유" + 버튼 잠김, 잠김 = "Lv.N", 구매 가능 = 가격 → 구매하면 보유
public class UnitShopPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator Shop_HeroAndMinion_Buy()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        Lobby.LobbyManager.Instance.OpenCanvas(LobbyState.Shop);
        yield return WaitFrames(3);
        var shop = Object.FindFirstObjectByType<ShopCanvas>(FindObjectsInactive.Include);
        Assert.IsTrue(shop != null && shop.gameObject.activeInHierarchy, "상점이 안 열림");
        var panels = shop.GetComponentsInChildren<UnitShopPanel>(true);
        var heroPanels = panels.Where(p => p.kind == UnitShopPanel.UnitKind.Hero).ToArray();
        var minionPanels = panels.Where(p => p.kind == UnitShopPanel.UnitKind.Minion).ToArray();
        Assert.AreEqual(HeroCollectionContainer.ShownHeroes().Count, heroPanels.Length, "용사 칸 수");
        Assert.AreEqual(GameSetting.MINION_LEVEL_UNLOCKS.Length, minionPanels.Length, "미니언 칸 수 (레벨 해금 미니언)");
        Assert.IsTrue(panels.All(p => p.gameObject.activeInHierarchy), "칸이 안 보임");
        Assert.IsTrue(shop.productContainers.Any(c => c.GetComponentInChildren<UnitShopPanel>(true) != null), "상점 줄 목록에 용사·미니언 줄");

        // 보유 용사 = "보유", 버튼 잠김
        var heroMgr = UserDataManager.Instance.userHeroManager;
        UnitShopPanel hero = heroPanels.First(p => heroMgr.IsOwned(p.key));
        hero.UpdatePanel();
        Assert.AreEqual(TranslateManager.GetText("Shop_Owned"), hero.priceDisplay.valueText.text);
        Assert.IsFalse(hero.buyButton.interactable);
        // 보유를 끄면 구매 가능 → 구매 → 보유
        heroMgr.GetUserHero(hero.key).own = false;
        hero.UpdatePanel();
        Assert.IsTrue(hero.buyButton.interactable, "미보유 용사 = 해금 가능");
        MinionBuyResult result = MinionBuyResult.Locked;
        hero.Buy(r => result = r);
        yield return WaitFrames(3); // 광고(테스트 중엔 바로 성공)·결제 콜백
        Assert.AreEqual(MinionBuyResult.Success, result);
        Assert.IsTrue(heroMgr.IsOwned(hero.key));
        Assert.AreEqual(TranslateManager.GetText("Shop_Owned"), hero.priceDisplay.valueText.text);

        // 미니언: 잠김 = "Lv.N" → 레벨을 올리면 해금 가능 → 해금 (2026-11-27 골드·다이아 대신 자동·광고·유료)
        var minionMgr = UserDataManager.Instance.userMinionManager;
        var info = UserDataManager.Instance.userMyInfoManager.userMyInfoData;
        int prevLv = info.lv;
        UnitShopPanel minion = minionPanels.First();
        minionMgr.GetUserMinion(minion.key).own = false;
        info.lv = 0;
        minion.UpdatePanel();
        if (UserMinionManager.GetUnlockLevel(minion.key) > 0)
        {
            StringAssert.StartsWith("Lv.", minion.priceDisplay.valueText.text, "잠김 = Lv.N");
            Assert.IsFalse(minion.buyButton.interactable);
        }
        info.lv = 99;
        minion.UpdatePanel();
        Assert.IsTrue(minion.buyButton.interactable, "레벨 도달 = 해금 가능");
        result = MinionBuyResult.Locked;
        minion.Buy(r => result = r);
        yield return WaitFrames(3);
        Assert.AreEqual(MinionBuyResult.Success, result);
        Assert.IsTrue(minionMgr.GetUserMinion(minion.key).own, "미니언 보유");
        info.lv = prevLv;
    }
}
