using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

// (2026-11-25) wave 중 하단 스킬 칸: 마우스를 올리면 설명, 손가락으로 길게 누르면 설명 + 그 클릭은 스킬 안 씀
public class HeroSkillTooltipPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator SkillTooltip_HoverAndLongPress()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
    }

    IEnumerator Body()
    {
        HeroSkillHud hud = Object.FindFirstObjectByType<HeroSkillHud>(FindObjectsInactive.Include);
        Assert.IsNotNull(hud, "HeroSkillHud 없음");
        hud.gameObject.SetActive(true);
        yield return WaitFrames(2);
        var trigger = hud.slots[0].button.GetComponent<HeroSkillTooltipTrigger>();
        Assert.IsNotNull(trigger, "스킬 칸에 설명 입력 없음");

        // 마우스 올리기 → 바로 보임, 나가면 숨김
        var mouse = new PointerEventData(EventSystem.current) { pointerId = -1 };
        trigger.OnPointerEnter(mouse);
        Assert.IsTrue(hud.tooltipRoot != null && hud.tooltipRoot.gameObject.activeSelf, "마우스 올려도 설명 안 보임");
        Assert.IsNotEmpty(hud.tooltipTitleText.text);
        Assert.IsNotEmpty(hud.tooltipDescText.text);
        trigger.OnPointerExit(mouse);
        Assert.IsFalse(hud.tooltipRoot.gameObject.activeSelf, "마우스가 나가도 설명이 남음");

        // 손가락: 짧게 누르면 안 보이고, 길게 누르면 보임 + 그 클릭은 스킬 안 씀
        var touch = new PointerEventData(EventSystem.current) { pointerId = 0 };
        trigger.OnPointerDown(touch);
        yield return WaitRealtime(0.1f);
        Assert.IsFalse(hud.tooltipRoot.gameObject.activeSelf, "짧게 눌렀는데 설명이 보임");
        yield return WaitRealtime(hud.tooltipHoldTime);
        Assert.IsTrue(hud.tooltipRoot.gameObject.activeSelf, "길게 눌러도 설명 안 보임");
        trigger.OnPointerUp(touch);
        Assert.IsFalse(hud.tooltipRoot.gameObject.activeSelf, "손을 떼도 설명이 남음");
        Assert.IsTrue(trigger.ConsumeLongPress(), "길게 누른 클릭이 스킬로 처리됨");
        Assert.IsFalse(trigger.ConsumeLongPress(), "다음 클릭까지 막힘");
        AssertNoErrors();
    }
}
