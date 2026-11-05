using UnityEngine;

// 하위에 미리 배치한 KSUpgradePanel들 (StatType은 각 패널 인스펙터에서 지정)
public class KSUpgradeContainer : KSMContainer
{
    KSUpgradePanel[] upgradePanels;

    public override void OpenContainer()
    {
        upgradePanels ??= GetComponentsInChildren<KSUpgradePanel>(true);
    }

    public override void UpdateContainer()
    {
        foreach (KSUpgradePanel panel in upgradePanels)
            panel.UpdatePanel();
    }
}
