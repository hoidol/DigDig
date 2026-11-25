using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 킹 선택 시 유닛 정보 패널: (2026-11-23) 용사 칸(미선택 = 흐리게) + 설명(용사 설명·에너지, 스크롤), 미니언 선택 시 숨김
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

        // 1) (2026-11-23) 용사 미선택 → 첫 칸(용사) 흐리게, 직업 칸은 숨김
        panel.SetMinion(king);
        yield return WaitFrames(2);
        Assert.IsTrue(info.raceBadge.gameObject.activeSelf);
        Assert.IsFalse(info.jobBadge.gameObject.activeSelf, "직업 칸은 숨김 (용사 하나)");
        Assert.Less(info.raceBadge.alpha, 1f, "용사 미선택인데 칸이 켜짐");
        Assert.IsFalse(info.nameText.gameObject.activeSelf, "킹 선택 시 이름 글씨는 숨김");

        // 2) 기사 고름 → 칸 켜짐, 설명 = 용사 설명 + 에너지(투지) 개수
        var setting = KingGrowthSetting.Instance;
        AbilityData knight = setting.heroAbilities.First(d => d.prefab is KnightHero);
        king.abilityInventory.Add(knight);
        panel.SetMinion(king);
        yield return WaitFrames(2);
        Assert.AreEqual(1f, info.raceBadge.alpha);
        Assert.AreEqual(knight.title, info.raceText.text);
        StringAssert.Contains($"{knight.title}</b> : ", info.descText.text);
        StringAssert.Contains("투지 0/0", info.descText.text); // (2026-11-24) 에너지 이름 = 용사마다

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
