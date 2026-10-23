using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SlimeShortcutInfoPanel : MonoBehaviour
{

    // public TMP_Text descText;

    public SlimeStatPanel[] slimeStatPanels;
    List<SlimeStatType> slimeStatTypes = new List<SlimeStatType>();
    public TMP_Text nameText;
    public TMP_Text descText;
    public void SetSlime(Slime slime)
    {
        gameObject.SetActive(true);
        nameText.text = slime.slimeData.Title;
        descText.text = slime.slimeData.GetDescription(slime.mergeLevel);
        // descText.text = slime.slimeData.GetDescription(slime.mergeLevel); 
        for (int i = 0; i < slimeStatPanels.Length; i++)
        {
            slimeStatPanels[i].gameObject.SetActive(false);
        }
        slimeStatTypes.Clear();

        slimeStatTypes.AddRange(slime.slimeData.displayStatTypes);
        slimeStatTypes.Remove(SlimeStatType.AttackRange);
        slimeStatTypes.Remove(SlimeStatType.AttackSpeed);

        for (int i = 0; i < slimeStatPanels.Length; i++)
        {
            if (i < slime.slimeData.displayStatTypes.Length)
            {
                slimeStatPanels[i].gameObject.SetActive(true);
                slimeStatPanels[i].SetSlimeStat(slime.slimeData.GetSlimeStat(slime.slimeData.displayStatTypes[i], slime.userSlime.enhanceLevel), slime.mergeLevel, slime.slimeData.GetRandomRatio(slime.slimeData.displayStatTypes[i]));
            }
            else
            {
                slimeStatPanels[i].gameObject.SetActive(false);

            }

        }
    }
}
