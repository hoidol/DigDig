using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// (2026-11-25) 기기 테스트 패널 (MobileTestPanel): 만들어지고, 버튼을 전부 눌러도 에러가 없고, 미니언 소환·용사 장착이 되는지
public class MobileTestPanelPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator Panel_AllButtons_Work()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        var panel = new GameObject("MobileTestPanel").AddComponent<MobileTestPanel>();
        yield return WaitFrames(2);
        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        Assert.Greater(buttons.Length, 10, "버튼 생성");
        Button Find(string label) => buttons.First(b => b.GetComponentInChildren<TMP_Text>(true).text.Contains(label));

        Find("TEST").onClick.Invoke(); // 패널 열기
        yield return WaitFrames(1);

        int before = MinionFieldController.Instance.activeminions.Count;
        Find("소환 (빈 칸)").onClick.Invoke();
        yield return WaitFrames(2);
        Assert.AreEqual(before + 1, MinionFieldController.Instance.activeminions.Count, "미니언 소환");

        Find("용사 장착").onClick.Invoke();
        yield return WaitFrames(2);
        Assert.IsNotNull(Slime.Instance.abilityInventory.Get<HeroSlimeAbility>(), "용사 장착");

        // (2026-11-26) 웨이브 전환: 시작 → 정지 = 다음 wave 브레이크타임 + 저장 / 이전 웨이브 = wave 1로 복구
        GameManager gm = GameManager.Instance;
        int startNext = gm.breakTimeNextWaveIdx;
        Find("웨이브 시작").onClick.Invoke();
        yield return WaitFrames(3);
        Assert.IsFalse(gm.isBreakTime, "웨이브 시작");
        Find("정지 (적 제거").onClick.Invoke();
        yield return WaitFrames(3);
        Assert.IsTrue(gm.isBreakTime, "정지 → 브레이크타임");
        Assert.AreEqual(startNext + 1, gm.breakTimeNextWaveIdx, "정지 → 다음 wave");
        Assert.AreEqual(0, EnemyFieldController.Instance.GetTargetEnemies().Count, "정지 → 적 제거");
        Assert.IsTrue(WaveSnapshotSystem.Has(startNext) && WaveSnapshotSystem.Has(startNext + 1), "브레이크타임마다 저장");
        Find("이전 웨이브").onClick.Invoke();
        yield return WaitFrames(3);
        Assert.AreEqual(startNext, gm.breakTimeNextWaveIdx, "이전 웨이브 복구");

        // 나머지 버튼 전부 (TEST 제외)
        foreach (Button b in buttons)
        {
            if (b.GetComponentInChildren<TMP_Text>(true).text == "TEST")
                continue;
            b.onClick.Invoke();
            yield return WaitFrames(1);
        }
        Time.timeScale = 1f;
        Object.Destroy(panel.gameObject);
        yield return WaitFrames(1);
    }
}
