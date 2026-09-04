using UnityEngine;
using TMPro;
public class LevelUpSlimeSlotPanel : MonoBehaviour
{
    public SlimePanel slimePanel;
    public TMP_Text levelText;
    public void SetSlime(Slime slime)
    {
        slimePanel.SetSlime(slime);
        levelText.text = $"Lv.{slime.level} > {slime.level + 1}";
    }
}