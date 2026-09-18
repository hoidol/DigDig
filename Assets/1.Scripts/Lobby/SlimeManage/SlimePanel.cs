
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lobby
{
    public class SlimePanel : MonoBehaviour
    {
        public TMP_Text titleText;
        public TMP_Text descText;
        public Image thumImge;
        public TMP_Text enhanceLvText;
        public SlimeExpPanel mineMeExpPanel;
        public SlimeGradePanel slimeGradePanel;

        public void SetData(SlimeData uMData)
        {
            UserSlime userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(uMData.key);
            if (titleText != null)
                titleText.text = uMData.Title;
            if (descText != null)
                descText.text = uMData.GetDescription();

            if (thumImge != null)
            {
                thumImge.sprite = uMData.thum;
                if (userSlime.own)
                {
                    thumImge.color = Color.white;
                }
                else
                {
                    thumImge.color = Color.gray;
                }
            }
                
            if(slimeGradePanel != null){
                slimeGradePanel.SetSlimeData(uMData);
            }

            if (enhanceLvText != null)
            {
                int enhanceLevel = userSlime.enhanceLevel;
                if(enhanceLevel == SlimeData.MAX_ENHANCE_LEVEL)
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
                mineMeExpPanel.SetSlime(uMData);    
            }
            
        }

    }
}
