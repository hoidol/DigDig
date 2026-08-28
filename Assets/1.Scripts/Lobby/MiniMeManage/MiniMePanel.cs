
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lobby
{
    public class MiniMePanel : MonoBehaviour
    {
        public TMP_Text titleText;
        public TMP_Text descText;
        public Image thumImge;
        public TMP_Text enhanceLvText;
        public MineMeExpPanel mineMeExpPanel;


        public void SetData(MiniMeData uMData)
        {
            UserMiniMe userMiniMe = UserManager.Instance.userMiniMeManager.GetUserMiniMe(uMData.key);
            titleText.text = uMData.Title;
            if(descText != null)
                descText.text = uMData.GetDescription();

            if(thumImge != null)
                thumImge.sprite =uMData.thum;

            if(enhanceLvText != null)
            {
                int enhanceLevel = userMiniMe.EnhanceLevel();
                enhanceLvText.text = $"LV.{enhanceLevel}";
            }
                

            mineMeExpPanel.SetMiniMe(uMData);
        }

    }    
}
