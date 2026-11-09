using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 로비 유물 선택 (CLAUDE.md "유물"): 묶음 6줄 × 3칸, 잠김 표시, 칸 → 정보 창 → 선택, 같은 묶음은 교체, 잠긴 묶음은 선택 불가
public class RelicPlayTest : InGamePlayTestBase
{
    static UserRelicManager Relics => UserDataManager.Instance.userRelicManager;

    [UnityTest]
    public IEnumerator Lobby_SelectRelicPerTier()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        yield return Lobby_SelectRelicPerTierBody();
    }

    IEnumerator Lobby_SelectRelicPerTierBody()
    {
        // Lv.10 = 묶음 1·2 해금, 3~6 잠김. 선택 초기화
        UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv = 10;
        Relics.userRelicData.selectedKeys = new string[RelicData.TierCount];

        Lobby.LobbyManager.Instance.OpenRelicSelect();
        yield return WaitFrames(3);
        var ks = Object.FindFirstObjectByType<KSManagementCanvas>(FindObjectsInactive.Include);
        Assert.IsTrue(ks.relicContainer.gameObject.activeInHierarchy, "유물 탭이 안 열림");

        var slots = ks.relicContainer.slotPanels;
        Assert.AreEqual(RelicData.TierCount * RelicData.RELICS_PER_TIER, slots.Length, "목록 칸 수 (묶음 × 3)");
        for (int i = 0; i < slots.Length; i++)
        {
            int tier = i / RelicData.RELICS_PER_TIER + 1;
            Assert.IsNotNull(slots[i].relicData, $"{i}번 칸 유물 없음");
            Assert.AreEqual(tier, slots[i].relicData.tier, $"{i}번 칸 묶음 (한 줄 = 한 묶음)");
            Assert.AreEqual(tier > 2, slots[i].lockPanel.activeSelf, $"{i}번 칸 잠김 표시 (Lv.10)");
            Assert.IsFalse(slots[i].selectedPanel.activeSelf, $"{i}번 칸: 고르지 않았는데 선택됨 표시");
        }

        // 묶음 1: 칸 클릭 → 정보 창 → 선택
        slots[0].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return WaitFrames(2);
        var info = RelicInfoCanvas.Instance;
        Assert.IsTrue(info.gameObject.activeInHierarchy, "칸을 눌러도 정보 창이 안 열림 (버튼 onClick 연결)");
        Assert.AreEqual(slots[0].relicData.Desc, info.relicInfoPanel.descText.text, "설명 표시");
        info.selectButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return WaitFrames(2);
        Assert.AreEqual(slots[0].relicData.key, Relics.GetSelectedKey(1), "묶음 1 선택");
        Assert.IsTrue(info.selectedPanel.activeSelf, "정보 창 선택됨 표시");
        Assert.IsFalse(info.selectButton.activeSelf, "선택한 유물인데 선택 버튼이 보임");
        Assert.IsTrue(slots[0].selectedPanel.activeSelf, "목록 선택됨 표시");

        // 같은 묶음의 다른 유물 → 교체
        info.OpenCanvas(slots[1].relicData);
        info.OnClickedSelectButton();
        yield return WaitFrames(2);
        Assert.AreEqual(slots[1].relicData.key, Relics.GetSelectedKey(1), "같은 묶음은 교체");
        Assert.IsFalse(slots[0].selectedPanel.activeSelf, "이전 선택 표시가 남음");
        Assert.IsTrue(slots[1].selectedPanel.activeSelf);

        // 잠긴 묶음 3 → 선택 안 됨
        info.OpenCanvas(slots[6].relicData);
        info.OnClickedSelectButton();
        yield return WaitFrames(2);
        Assert.IsNull(Relics.GetSelectedKey(3), "잠긴 묶음이 선택됨");

        // 묶음 2 선택 → 적용 유물 2개, 아직 안 고른 해금 묶음 없음
        Assert.AreEqual(new[] { 2 }, Relics.GetUnselectedUnlockedTiers().ToArray(), "해금됐는데 안 고른 묶음");
        Assert.AreEqual(RelicSelectResult.Success, Relics.Select(slots[3].relicData));
        Assert.AreEqual(new[] { slots[1].relicData.key, slots[3].relicData.key }, Relics.GetActiveRelicKeys().ToArray(), "적용 유물 = 해금된 묶음의 선택 전부");
        Assert.IsEmpty(Relics.GetUnselectedUnlockedTiers());

        // 레벨이 내려가 잠긴 묶음의 선택은 적용 안 됨 (데이터는 유지)
        UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv = 5;
        Assert.AreEqual(new[] { slots[1].relicData.key }, Relics.GetActiveRelicKeys().ToArray(), "Lv.5면 묶음 1만 적용");
        AssertNoErrors();
    }
}
