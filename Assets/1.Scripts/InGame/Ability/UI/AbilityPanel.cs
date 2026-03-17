using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AbilityPanel : MonoBehaviour
{
    [SerializeField] AbilityData abilityData;

    public Image thumImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public void SetAbilityData(AbilityData abilityData)
    {
        this.abilityData = abilityData;
        titleText.text = abilityData.Title;
        descriptionText.text = abilityData.Description();
    }

    public void OnClickedSelect()
    {
        Player.Instance.AddAbility(this.abilityData.key);
        AbilityCanvas.Instance.CloseCanvas();
    }
}
