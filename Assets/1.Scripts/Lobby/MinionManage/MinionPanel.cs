
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lobby
{
    public class MinionPanel : MonoBehaviour
    {
        public TMP_Text titleText;
        public TMP_Text descText;
        public Image thumImge;
        public TMP_Text enhanceLvText;
        public MinionExpPanel mineMeExpPanel;
        public virtual void SetData(MinionData uMData)
        {
            UserMinion userMinion = UserDataManager.Instance.userMinionManager.GetUserMinion(uMData.key);
            if (titleText != null)
                titleText.text = uMData.Title;
            if (descText != null)
                descText.text = uMData.GetDescription();

            if (thumImge != null)
            {
                thumImge.SetThum(uMData.thum); // SetNativeSize 후 칸 크기에 맞춤 (MinionEquipedSlotPanel.thumImage와 같은 방식)
                if (userMinion.own)
                {
                    thumImge.color = Color.white;
                }
                else
                {
                    thumImge.color = Color.black;
                }
            }

            if (enhanceLvText != null)
            {
                int enhanceLevel = userMinion.EnhanceLevel;
                EnhanceExpInfo expInfo = MinionManager.Instance.GetEnhanceExpInfo();
                if (expInfo != null && expInfo.IsMaxLevel(enhanceLevel))
                {
                    enhanceLvText.text = "MAX";
                }
                else
                {
                    enhanceLvText.text = enhanceLevel.ToString();
                }

            }

            if (mineMeExpPanel != null)
            {
                mineMeExpPanel.SetMinion(uMData);
            }

        }

    }
}
