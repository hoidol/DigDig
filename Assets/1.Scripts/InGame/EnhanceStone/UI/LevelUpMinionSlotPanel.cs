using UnityEngine;
using TMPro;
public class LevelUpMinionSlotPanel : MonoBehaviour
{
    public MinionPanel minionPanel;
    public TMP_Text levelText;
    public void SetMinion(Minion minion)
    {
        minionPanel.SetMinion(minion);
        levelText.text = $"Lv.{minion.mergeLevel} > {minion.mergeLevel + 1}";
    }
}