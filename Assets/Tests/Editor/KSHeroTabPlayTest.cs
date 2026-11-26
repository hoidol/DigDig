using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-26) 로비 킹 관리 화면 [용사] 탭: HeroData 용사 3열 그리드, 보유 용사를 누르면 킹슬라임 자리에 용사 정보, 다시 누르면 해제
// [수련] 탭 = 레벨 스탯. 미보유 용사는 눌러도 반응 없음
public class KSHeroTabPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator HeroTab_SelectAndRelease()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        Lobby.LobbyManager.Instance.OpenCanvas(LobbyState.KingSlimeManagement);
        yield return WaitFrames(3);
        var canvas = Object.FindFirstObjectByType<KSManagementCanvas>(FindObjectsInactive.Include);
        Assert.IsTrue(canvas != null && canvas.gameObject.activeInHierarchy, "KSManagementCanvas가 안 열림");
        Assert.IsNotNull(canvas.heroContainer, "용사 탭 창");

        // 탭 글씨: 용사 / 수련
        var tabs = canvas.GetComponentsInChildren<KSMMenuTapButton>(true);
        Assert.AreEqual(2, tabs.Length);
        StringAssert.Contains(TranslateManager.GetText("KSM_Tab_Hero"), tabs.First(t => t.menuType == KSMenuType.Hero).GetComponentInChildren<TMPro.TMP_Text>(true).text);
        StringAssert.Contains(TranslateManager.GetText("KSM_Tab_Training"), tabs.First(t => t.menuType == KSMenuType.LevelStat).GetComponentInChildren<TMPro.TMP_Text>(true).text);

        // 처음 = 용사 탭: 용사 그리드 3열, 수련 창 숨김
        Assert.IsTrue(canvas.heroContainer.gameObject.activeInHierarchy, "처음 = 용사 탭");
        Assert.IsFalse(canvas.levelStatContainer.gameObject.activeInHierarchy);
        var grid = canvas.heroContainer.content.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        Assert.AreEqual(HeroCollectionContainer.COLUMN_COUNT, grid.constraintCount, "3열");
        var heroes = HeroCollectionContainer.ShownHeroes();
        var slots = canvas.heroContainer.content.GetComponentsInChildren<HeroSlotPanel>().Where(s => s.gameObject.activeSelf).ToArray();
        Assert.AreEqual(heroes.Count, slots.Length, "HeroData 용사 수만큼 칸");
        Assert.Greater(slots.Length, 0);

        // 보유 용사를 누르면 용사 정보, 킹슬라임 숨김
        Assert.IsTrue(canvas.kingSlimeBody == null || canvas.kingSlimeBody.activeSelf, "처음엔 킹슬라임");
        int lv = HeroCollectionContainer.UserLevel;
        int ownedIdx = heroes.FindIndex(h => HeroCollectionContainer.IsOwned(h, lv));
        Assert.GreaterOrEqual(ownedIdx, 0, "보유 용사 없음");
        var hero = heroes[ownedIdx];
        slots[ownedIdx].button.onClick.Invoke();
        yield return WaitFrames(2);
        Assert.AreEqual(hero.key, canvas.heroContainer.SelectedKey);
        Assert.IsTrue(canvas.heroPreview != null && canvas.heroPreview.gameObject.activeSelf, "용사 정보");
        if (canvas.kingSlimeBody != null)
            Assert.IsFalse(canvas.kingSlimeBody.activeSelf, "용사 정보 동안 킹슬라임 숨김");
        Assert.AreEqual(hero.desc, canvas.heroPreviewDesc.text);
        // 능력치 칸 = 용사 Lv.1 공격력 (+ 수련 포인트)
        var atkPanel = canvas.kSStatStateContainer.currentKSAbilityInfoPanels.FirstOrDefault(p => p != null && p.statType == StatType.AttackPower);
        if (atkPanel != null && hero.TryGetStat(StatType.AttackPower, 1, out float heroAtk))
            Assert.AreEqual(StatData.GetValueToString(StatType.AttackPower, KingSlimeManager.Instance.GetTotalStatValue(heroAtk, StatType.AttackPower)), atkPanel.valueText.text, "용사 공격력 표시");

        // 다시 누르면 해제 → 킹슬라임
        slots[ownedIdx].button.onClick.Invoke();
        yield return WaitFrames(2);
        Assert.IsNull(canvas.heroContainer.SelectedKey, "다시 누르면 해제");
        Assert.IsFalse(canvas.heroPreview.gameObject.activeSelf);
        if (canvas.kingSlimeBody != null)
            Assert.IsTrue(canvas.kingSlimeBody.activeSelf, "해제하면 킹슬라임");

        // 미보유 용사는 반응 없음 (있을 때만)
        int lockedIdx = heroes.FindIndex(h => !HeroCollectionContainer.IsOwned(h, lv));
        if (lockedIdx >= 0)
        {
            slots[lockedIdx].button.onClick.Invoke();
            yield return WaitFrames(2);
            Assert.IsNull(canvas.heroContainer.SelectedKey, "미보유 용사는 선택 안 됨");
        }

        // (2026-11-26) 보유 = UserHeroManager.own: 무료 용사는 자동 보유, 보유를 끄면 선택 안 됨 → 다시 보유하면 선택됨
        var heroMgr = UserDataManager.Instance.userHeroManager;
        foreach (var h in heroes.Where(h => h.buyPrice <= 0))
            Assert.IsTrue(heroMgr.IsOwned(h.key), $"무료 용사 {h.key} 자동 보유");
        Assert.AreEqual(MinionBuyResult.AlreadyOwned, heroMgr.BuyHero(hero.key), "보유한 용사는 다시 못 삼");
        heroMgr.GetUserHero(hero.key).own = false;
        canvas.heroContainer.Refresh();
        slots[ownedIdx].button.onClick.Invoke();
        yield return WaitFrames(2);
        Assert.IsNull(canvas.heroContainer.SelectedKey, "보유하지 않으면 선택 안 됨");
        Assert.AreEqual(MinionBuyState.Buyable, heroMgr.GetBuyState(hero.key), "해금 레벨 도달 + 미보유 = 구매 가능");
        Assert.AreEqual(MinionBuyResult.Success, heroMgr.BuyHero(hero.key), "가격 없는 용사 구매 = 바로 보유");
        Assert.IsTrue(heroMgr.IsOwned(hero.key));
        canvas.heroContainer.Refresh();
        slots[ownedIdx].button.onClick.Invoke();
        yield return WaitFrames(2);
        Assert.AreEqual(hero.key, canvas.heroContainer.SelectedKey, "다시 보유하면 선택됨");
        slots[ownedIdx].button.onClick.Invoke();
        yield return WaitFrames(2);

        // 수련 탭
        tabs.First(t => t.menuType == KSMenuType.LevelStat).OnClickedBtn();
        yield return WaitFrames(2);
        Assert.IsTrue(canvas.levelStatContainer.gameObject.activeInHierarchy, "수련 탭 = 레벨 스탯");
        Assert.IsFalse(canvas.heroContainer.gameObject.activeInHierarchy);
    }
}
