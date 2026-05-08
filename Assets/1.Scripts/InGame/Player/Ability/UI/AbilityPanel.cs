using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
public class AbilityPanel : MonoBehaviour
{
    [SerializeField] AbilityData abilityData;

    public Image thumImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public GameObject closeObject;
    public void SetAbilityData(AbilityData abilityData)
    {
        closeObject.gameObject.SetActive(abilityData == null);
        if (abilityData == null)
        {
            return;
        }

        this.abilityData = abilityData;
        titleText.text = abilityData.Title;
        descriptionText.text = abilityData.Description();
    }

    public void OnClickedSelect()
    {
        Player.Instance.abilityInventory.AddAbility(this.abilityData.key);
        AbilityCanvas.Instance.CloseCanvas();
    }
}
