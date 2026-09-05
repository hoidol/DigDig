
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


        public void SetData(SlimeData uMData)
        {
            UserSlime userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(uMData.key);
            if (titleText != null)
                titleText.text = uMData.Title;
            if (descText != null)
                descText.text = uMData.GetDescription();

            if (thumImge != null)
                thumImge.sprite = uMData.thum;

            if (enhanceLvText != null)
            {
                int enhanceLevel = userSlime.EnhanceLevel();
                enhanceLvText.text = $"LV.{enhanceLevel}";
            }


            mineMeExpPanel.SetSlime(uMData);
        }

    }
}
