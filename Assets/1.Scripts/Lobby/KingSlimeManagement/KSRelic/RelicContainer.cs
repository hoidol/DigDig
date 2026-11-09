using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// KS 관리 화면 유물 메뉴: 묶음 6줄 × 3칸 (줄 = 해금 레벨 5·10·15·20·25·30)
public class RelicContainer : KSMContainer
{
    public RelicSlotPanel[] slotPanels; // 묶음 순서대로 RELICS_PER_TIER개씩

    public override void UpdateContainer()
    {
        for (int tier = 1; tier <= RelicData.TierCount; tier++)
        {
            List<RelicData> relics = RelicManager.Instance.GetTierRelics(tier);
            for (int i = 0; i < RelicData.RELICS_PER_TIER; i++)
            {
                int idx = (tier - 1) * RelicData.RELICS_PER_TIER + i;
                if (idx < slotPanels.Length)
                    slotPanels[idx].SetRelic(i < relics.Count ? relics[i] : null);
            }
        }
    }
}
