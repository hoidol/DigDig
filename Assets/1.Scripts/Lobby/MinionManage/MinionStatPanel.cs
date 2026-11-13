
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    public class MinionStatPanel : MonoBehaviour
    {
        // public MinionStatType statType;
        public TMP_Text titleText;
        public TMP_Text valueText;
        public Image thumImage;
        public void SetMinionStat(MinionStat minionStat, int mergeLv, float randomRatio = 0f)
        {
            if (thumImage != null)
                thumImage.sprite = minionStat.Thum;

            titleText.text = minionStat.Title;
            valueText.text = minionStat.GetValueToString(mergeLv, randomRatio);
        }
    }

}