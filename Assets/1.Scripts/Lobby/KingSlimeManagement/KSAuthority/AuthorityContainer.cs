using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// KS 관리 화면 권능 메뉴: 묶음 6줄 × 3칸 (줄 = 해금 레벨 5·10·15·20·25·30)
public class AuthorityContainer : KSMContainer
{
    public AuthoritySlotPanel[] slotPanels; // 묶음 순서대로 AUTHORITIES_PER_TIER개씩

    public override void UpdateContainer()
    {
        for (int tier = 1; tier <= AuthorityData.TierCount; tier++)
        {
            List<AuthorityData> authorities = AuthorityManager.Instance.GetTierAuthorities(tier);
            for (int i = 0; i < AuthorityData.AUTHORITIES_PER_TIER; i++)
            {
                int idx = (tier - 1) * AuthorityData.AUTHORITIES_PER_TIER + i;
                if (idx < slotPanels.Length)
                    slotPanels[idx].SetAuthority(i < authorities.Count ? authorities[i] : null);
            }
        }
    }
}
