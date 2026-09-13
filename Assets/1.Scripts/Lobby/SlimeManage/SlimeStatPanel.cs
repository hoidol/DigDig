using TMPro;
using UnityEngine;

public class SlimeStatPanel : MonoBehaviour
{
    public StatType statType;
    public TMP_Text valueText;
    public void SetSlimeData(SlimeData slimeData)
    {
        valueText.text = slimeData.GetSlimeStat(statType).GetValueToString();
    }
}
