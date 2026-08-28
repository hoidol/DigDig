
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
        public void SetData(MiniMeData uMData)
        {
            titleText.text = uMData.Title;
            descText.text = uMData.GetDescription();
            thumImge.sprite =uMData.thum;
        }

    }    
}
