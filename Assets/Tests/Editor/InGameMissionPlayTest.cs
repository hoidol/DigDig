using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// (2026-11-23) 피해 집계 버튼 아래 미션 버튼 → 누르면 미션 목록 창 (피해 집계 창과 번갈아)
public class InGameMissionPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator MissionButton_TogglesPanel()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        Assert.IsNotNull(InGameMissionSystem.Instance, "미션 시스템 없음");
        var missionButton = Object.FindFirstObjectByType<InGameMissionButton>(FindObjectsInactive.Include);
        Assert.IsNotNull(missionButton, "미션 버튼 없음");
        var damageButton = Object.FindFirstObjectByType<DamageMeterSystemButton>(FindObjectsInactive.Include);

        var missionRt = (RectTransform)missionButton.transform;
        var damageRt = (RectTransform)damageButton.transform;
        Assert.AreSame(damageRt.parent, missionRt.parent);
        Assert.Less(missionRt.anchoredPosition.y, damageRt.anchoredPosition.y, "피해 집계 버튼 아래");
        Assert.IsNull(missionButton.GetComponent<DamageMeterSystemButton>(), "복사한 피해 집계 기능은 지움");

        Assert.IsNotNull(missionButton.panel);
        Assert.IsFalse(missionButton.panel.activeSelf, "처음엔 닫힘");

        missionButton.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(2);
        Assert.IsTrue(missionButton.panel.activeSelf, "누르면 열림");
        Assert.IsFalse(damageButton.container.gameObject.activeSelf, "피해 집계 창은 안 열림");

        int count = InGameMissionSystem.Instance.entries.Count;
        var lines = missionButton.panel.GetComponentsInChildren<TMP_Text>(false)
            .Where(t => t.name != "Title").ToArray();
        Assert.AreEqual(count, lines.Length, "미션 수만큼 줄");
        foreach (var line in lines)
        {
            Assert.IsNotEmpty(line.text);
            Assert.IsFalse(line.text.Contains("{"), line.text);
        }

        damageButton.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(1);
        Assert.IsTrue(damageButton.container.gameObject.activeSelf, "피해 집계 창 열림");
        Assert.IsFalse(missionButton.panel.activeSelf, "피해 집계를 열면 미션 창은 닫힘");

        missionButton.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(1);
        Assert.IsTrue(missionButton.panel.activeSelf);
        Assert.IsFalse(damageButton.container.gameObject.activeSelf, "미션을 열면 피해 집계 창은 닫힘");

        missionButton.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(1);
        Assert.IsFalse(missionButton.panel.activeSelf, "다시 누르면 닫힘");
    }
}
