using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MinionShortcutInfoPanel : MonoBehaviour
{

    // public TMP_Text descText;

    public InGame.MinionStatPanel[] minionStatPanels;
    List<StatType> minionStatTypes = new List<StatType>();
    public TMP_Text nameText;
    public TMP_Text descText;
    public void SetMinion(Minion minion)
    {
        gameObject.SetActive(true);
        nameText.text = minion.minionData.Title;
        descText.text = minion.minionData.GetDescription(minion.mergeLevel);
        // descText.text = minion.minionData.GetDescription(minion.mergeLevel); 
        for (int i = 0; i < minionStatPanels.Length; i++)
        {
            minionStatPanels[i].gameObject.SetActive(false);
        }
        minionStatTypes.Clear();

        minionStatTypes.AddRange(minion.minionData.displayStatTypes);
        minionStatTypes.Remove(StatType.AttackRange);
        minionStatTypes.Remove(StatType.AttackSpeed);

        for (int i = 0; i < minionStatPanels.Length; i++)
        {
            if (i < minion.minionData.displayStatTypes.Count)
            {
                minionStatPanels[i].gameObject.SetActive(true);
                minionStatPanels[i].SetMinionStat(minion, minion.minionData.displayStatTypes[i]);
            }
            else
            {
                minionStatPanels[i].gameObject.SetActive(false);

            }

        }
    }
}
