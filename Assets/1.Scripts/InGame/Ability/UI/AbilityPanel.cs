using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AbilityPanel : MonoBehaviour
{
    AbilityData abilityData;

    public Image thumImage;
    public TMP_Text nameText;
    public void SetAbilityData(AbilityData abilityData)
    {
        this.abilityData = abilityData;
    }
}
