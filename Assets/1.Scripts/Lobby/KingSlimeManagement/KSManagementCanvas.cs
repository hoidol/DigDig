using System;
using UnityEngine;

// 오라(Aura)는 출시 후 업데이트에서 추가 예정 → 관련 코드는 주석 처리 (2026-11-05)
public class KSManagementCanvas : BaseLobbyCanvas
{
   public KSStatStateContainer kSStatStateContainer;
   // public KSAuraStateContainer kSAuraStateContainer;

    public KSUpgradeContainer upgradeContainer;

   public KSEquipmentContainer kSEquipmentContainer;
   // public KSAuraContainer kSAuraContainer;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        kSStatStateContainer.OpenContainer();
        // kSAuraStateContainer.OpenContainer();

        upgradeContainer.OpenContainer();
        kSEquipmentContainer.OpenContainer();
        // kSAuraContainer.OpenContainer();

        SelectMenu(KSMenuType.Upgrade);

        UpdateCanvas();
    }

    public void SelectMenu(KSMenuType menuType)
    {
        // 오라 업데이트 전까지 Aura 메뉴는 무시
        if (menuType == KSMenuType.Aura)
            return;

        // kSStatStateContainer.gameObject.SetActive(false);
        // kSAuraStateContainer.gameObject.SetActive(false);
        //
        // if(menuType == KSMenuType.Upgrade || menuType == KSMenuType.Equipment)
        // {
        //     kSStatStateContainer.gameObject.SetActive(true);
        // }
        // else
        // {
        //     kSAuraStateContainer.gameObject.SetActive(true);
        // }
        kSStatStateContainer.gameObject.SetActive(true);

        upgradeContainer.gameObject.SetActive(menuType == KSMenuType.Upgrade);
        kSEquipmentContainer.gameObject.SetActive(menuType == KSMenuType.Equipment);
        // kSAuraContainer.gameObject.SetActive(menuType == KSMenuType.Aura);
    }

    public override void UpdateCanvas()
    {
        kSStatStateContainer.UpdatePanel();
        // kSAuraStateContainer.UpdatePanel();

        upgradeContainer.UpdateContainer();
        kSEquipmentContainer.UpdateContainer();
        // kSAuraContainer.UpdateContainer();
    }
}
