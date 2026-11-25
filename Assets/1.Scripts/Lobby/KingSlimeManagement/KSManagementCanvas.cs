using System;
using UnityEngine;
using UnityEngine.Serialization;

// 킹슬라임 관리: 상단 능력치 + 레벨 스탯 (2026-11-24 강화·권능 탭 → 레벨 스탯 하나로 통합)
public class KSManagementCanvas : BaseLobbyCanvas
{
    public KSStatStateContainer kSStatStateContainer;
    [FormerlySerializedAs("authorityContainer")][FormerlySerializedAs("kSEquipmentContainer")]
    public LevelStatContainer levelStatContainer;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        kSStatStateContainer.OpenContainer();
        levelStatContainer.OpenContainer();
        levelStatContainer.gameObject.SetActive(true);

        UpdateCanvas();
    }

    // 메뉴 탭은 이제 하나뿐 (씬의 탭 버튼이 남아 있어도 레벨 스탯만 보임)
    public void SelectMenu(KSMenuType menuType)
    {
        levelStatContainer.gameObject.SetActive(true);
    }

    public override void UpdateCanvas()
    {
        kSStatStateContainer.UpdatePanel();
        levelStatContainer.UpdateContainer();
    }
}
