
using UnityEngine;

public class EquipmentStatePanel : MonoBehaviour
{
    public EquippedSlotPanel[] equippedSlotPanels;
    public CharacterBody characterBody;
    public CurrentEquipmentAbilityInfoPanel[] currentEquipmentAbilityInfoPanels; //현재 끼고 있는 장비 별 능력치 정보
    public void OpenPanel()
    {

    }
    public void UpdatePanel()
    {
        foreach (EquippedSlotPanel panel in equippedSlotPanels)
        {
            panel.UpdatePanel();
        }

        characterBody.UpdateCharacter();

        for (int i = 0; i < currentEquipmentAbilityInfoPanels.Length; i++)
        {
            currentEquipmentAbilityInfoPanels[i].UpdatePanel(UserManager.Instance.userData.characterName);
        }
    }
}