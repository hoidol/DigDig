using System;
using UnityEngine;
using UnityEngine.Serialization;

// 킹슬라임 관리: 상단 능력치 + 메뉴(강화 / 권능). 오라는 폐기(2026-11-09)
public class KSManagementCanvas : BaseLobbyCanvas
{
    public KSStatStateContainer kSStatStateContainer;
    public KSUpgradeContainer upgradeContainer;
    [FormerlySerializedAs("kSEquipmentContainer")] public AuthorityContainer authorityContainer;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        kSStatStateContainer.OpenContainer();
        upgradeContainer.OpenContainer();
        authorityContainer.OpenContainer();

        SelectMenu(KSMenuType.Upgrade);

        UpdateCanvas();
    }

    public void SelectMenu(KSMenuType menuType)
    {
        upgradeContainer.gameObject.SetActive(menuType == KSMenuType.Upgrade);
        authorityContainer.gameObject.SetActive(menuType == KSMenuType.Authority);
    }

    public override void UpdateCanvas()
    {
        kSStatStateContainer.UpdatePanel();
        upgradeContainer.UpdateContainer();
        authorityContainer.UpdateContainer();
    }
}
