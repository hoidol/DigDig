using TMPro;
using UnityEngine;

//EquipmentCompareCanvas에서 CurEquipment 보여줄때 사용하기 + 좋아지는 안좋아지는지 표시
public class EquipmentAbilityCompareInfoPanel : EquipmentAbilityInfoPanel
{
    public GameObject upImage;
    public GameObject downImage;
    public override void UpdatePanel(EquipmentData equipmentData)
    {
        EquipmentAbility equipmentAbility = equipmentData.GetEquipmentAbility(statType);
        
        if (equipmentAbility != null)
        {
            upImage.SetActive(false);
            downImage.SetActive(false);

            gameObject.SetActive(true);
            titleText.text = equipmentAbility.Title;
            valueText.text = equipmentAbility.GetValueToString();

            UserEquipment equippedUserEquipment = UserManager.Instance.userEquipmentManager.GetEquippedUserEquipment(equipmentData.equipmentType);
            if(equippedUserEquipment!= null)
            {
                EquipmentAbility equippedEquipmentAbility = equippedUserEquipment.equipmentData.GetEquipmentAbility(statType);
                if(equipmentAbility.value > equippedEquipmentAbility.value)
                {
                    upImage.SetActive(true);
                }
                else if(equipmentAbility.value  < equippedEquipmentAbility.value) 
                {
                    downImage.SetActive(true);
                }
            }

        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}