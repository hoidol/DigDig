using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-22) 적 정보 패널 i 버튼 → 방어·공격 상성표: 5×5칸(머리 포함), 값 = DefenceInfo 배율, 바깥·X로 닫힘
public class DefenceTableCanvasPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator InfoButton_OpensTable()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        var panel = WaveEnemyInfoPanel.Instance;
        Assert.IsNotNull(panel.infoButton, "인포 버튼 없음");
        panel.Show(2); // wave 3
        yield return WaitFrames(1);
        panel.infoButton.onClick.Invoke();
        yield return WaitFrames(2);
        var canvas = DefenceTableCanvas.Instance;
        Assert.IsTrue(canvas.gameObject.activeSelf, "상성표가 안 열림");
        var texts = canvas.gridRoot.GetComponentsInChildren<TMP_Text>().Select(t => t.text).ToList();
        Assert.AreEqual(25, canvas.gridRoot.childCount, "머리 줄·칸 포함 5×5");
        string physicVsLight = $"{Mathf.RoundToInt(DefenceInfo.Get(AttackType.Physic, DefenceType.Light) * 100)}%";
        Assert.IsTrue(texts.Any(t => t.Contains(physicVsLight)), "배율표 값 표시");
        Assert.IsTrue(texts.Contains(DefenceInfo.Title(DefenceType.Ethereal)) || texts.Any(t => t.Contains(DefenceInfo.Title(DefenceType.Ethereal))));

        canvas.dimButton.onClick.Invoke();
        yield return WaitFrames(1);
        Assert.IsFalse(canvas.gameObject.activeSelf, "바깥 누르면 닫힘");
        panel.infoButton.onClick.Invoke();
        yield return WaitFrames(1);
        canvas.closeButton.onClick.Invoke();
        yield return WaitFrames(1);
        Assert.IsFalse(canvas.gameObject.activeSelf, "X로 닫힘");
    }
}
