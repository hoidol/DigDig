
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
        public MinionGradePanel minionGradePanel;
        public Image bgImage;
        public Image innerBorder;
        public Image hightLightImage;
        public virtual void SetData(MinionData uMData)
        {
            UserMinion userMinion = UserDataManager.Instance.userMinionManager.GetUserMinion(uMData.key);
            if (titleText != null)
                titleText.text = uMData.Title;
            if (descText != null)
                descText.text = uMData.GetDescription();

            if (thumImge != null)
            {
                thumImge.sprite = uMData.thum;
                if (userMinion.own)
                {
                    thumImge.color = Color.white;
                }
                else
                {
                    thumImge.color = Color.gray;
                }
            }
                
            if(minionGradePanel != null){
                minionGradePanel.SetMinionData(uMData);
            }

            // 카드 틀 색 = 등급 (연결 안 된 이미지는 건너뜀)
            GradeCardColor cardColor = Grade.GetCardColor(uMData.grade);
            if (bgImage != null)
                bgImage.color = cardColor.bg;
            if (innerBorder != null)
                innerBorder.color = cardColor.innerBorder;
            if (hightLightImage != null)
                hightLightImage.color = cardColor.highLight;

            if (enhanceLvText != null)
            {
                int enhanceLevel = userMinion.enhanceLevel;
                if(enhanceLevel == MinionData.MAX_ENHANCE_LEVEL)
                {
                    enhanceLvText.text = "MAX";
                }   
                else
                {
                    enhanceLvText.text = enhanceLevel.ToString();    
                }
                
            }

            if(mineMeExpPanel != null)
            {
                mineMeExpPanel.SetMinion(uMData);    
            }
            
        }

    }
}
