using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

// (2026-11-21) wave 카드를 누르면 위에 미니언 설명, 같은 카드 다시 누르면 닫힘, 드래그를 시작하면 닫힘
public class MinionOfferCardInfoPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator CardClick_ShowsInfo_DragHides()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        var system = MinionOfferSystem.Instance;
        Assert.IsNotNull(system);
        yield return WaitFrames(3);
        var card = system.GetComponentsInChildren<MinionOfferCard>(false).FirstOrDefault(c => c.gameObject.activeInHierarchy);
        Assert.IsNotNull(card, "브레이크타임에 카드가 없음");
        var info = system.cardInfo;
        Assert.IsNotNull(info, "카드 설명 연결 없음");
        Assert.IsFalse(info.IsShown);

        var click = new PointerEventData(EventSystem.current) { dragging = false, position = Vector2.zero };
        card.OnPointerClick(click);
        yield return WaitFrames(1);
        Assert.IsTrue(info.IsShown, "누르면 설명이 보여야 함");
        Assert.IsNotEmpty(info.nameText.text);
        StringAssert.Contains("형", info.nameText.text, "공격 형태(발사형 등) 표시");
        Assert.Greater(info.panel.position.y, card.transform.position.y, "설명은 카드 위쪽");

        card.OnPointerClick(click);
        yield return WaitFrames(1);
        Assert.IsFalse(info.IsShown, "같은 카드를 다시 누르면 닫힘");

        card.OnPointerClick(click);
        yield return WaitFrames(1);
        card.OnBeginDrag(click);
        Assert.IsFalse(info.IsShown, "드래그를 시작하면 닫힘");
        card.OnEndDrag(click); // 판 밖에 놓음 → 카드 그대로
        yield return WaitFrames(1);
    }
}
