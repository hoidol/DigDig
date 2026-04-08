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
    public GameObject synergyNotifyObj;
    public SynergyItemPanel[] synergyItems;
    public void SetAbilityData(AbilityData abilityData)
    {
        this.abilityData = abilityData;
        titleText.text = abilityData.Title;
        descriptionText.text = abilityData.Description();
        synergyNotifyObj.SetActive(abilityData.synergyItems.Length > 0);
        for (int i = 0; i < synergyItems.Length; i++)
        {
            if (i < abilityData.synergyItems.Length)
            {
                synergyItems[i].synergyItemImage.sprite = ItemManager.Instance.GetItemData(abilityData.synergyItems[i]).thumbnail;
            }
            synergyItems[i].gameObject.SetActive(i < abilityData.synergyItems.Length);
        }
    }

    public void OnClickedSelect()
    {
        Player.Instance.abilityInventory.AddAbility(this.abilityData.key);
        AbilityCanvas.Instance.CloseCanvas();
    }
}
