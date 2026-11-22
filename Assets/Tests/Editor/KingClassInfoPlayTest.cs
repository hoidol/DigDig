using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-21) 킹 선택 시 유닛 정보 패널: 종족·직업 칸(미선택 = 흐리게) + 설명 "이름 : 설명" 두 줄(스크롤), 미니언 선택 시 숨김
public class KingClassInfoPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator KingSelect_ShowsRaceAndJob()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        KingSlime king = KingSlime.Instance;
        var panel = Object.FindObjectsByType<AllySettingPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None).First();
        var info = panel.kingClassInfoPanel;
        Assert.IsNotNull(info, "KingClassInfoPanel 연결 없음");
        Assert.IsNotNull(info.descScroll, "설명 스크롤 없음");

        // 1) 아무것도 안 고름 → 두 칸 흐리게
        panel.SetMinion(king);
        yield return WaitFrames(2);
        Assert.IsTrue(info.raceBadge.gameObject.activeSelf && info.jobBadge.gameObject.activeSelf);
        Assert.Less(info.raceBadge.alpha, 1f, "종족 미선택인데 칸이 켜짐");
        Assert.Less(info.jobBadge.alpha, 1f);
        Assert.IsFalse(info.nameText.gameObject.activeSelf, "킹 선택 시 이름 글씨는 숨김");

        // 2) 오크 + 전사 고름 → 칸 켜짐, 설명 두 줄
        var setting = KingGrowthSetting.Instance;
        AbilityData orc = setting.raceAbilities.First(d => ((RaceKingAbility)d.prefab).raceType == KingRaceType.Orc);
        AbilityData warrior = setting.jobAbilities.First(d => ((JobKingAbility)d.prefab).jobType == KingJobType.Warrior);
        king.abilityInventory.Add(orc);
        king.abilityInventory.Add(warrior);
        panel.SetMinion(king);
        yield return WaitFrames(2);
        Assert.AreEqual(1f, info.raceBadge.alpha);
        Assert.AreEqual(1f, info.jobBadge.alpha);
        Assert.AreEqual(orc.title, info.raceText.text);
        Assert.AreEqual(warrior.title, info.jobText.text);
        StringAssert.Contains($"{orc.title}</b> : ", info.descText.text);
        StringAssert.Contains($"{warrior.title}</b> : ", info.descText.text);

        // 3) 미니언 선택 → 칸 숨김, 이름 보임
        Tile tile = GetFreeTile();
        if (tile.isBlocked) tile.Unblock();
        Minion minion = SpawnMinion("Iron", tile);
        yield return WaitFrames(2);
        panel.SetMinion(minion);
        yield return WaitFrames(2);
        Assert.IsFalse(info.raceBadge.gameObject.activeSelf);
        Assert.IsFalse(info.jobBadge.gameObject.activeSelf);
        Assert.IsTrue(info.nameText.gameObject.activeSelf);
    }
}
