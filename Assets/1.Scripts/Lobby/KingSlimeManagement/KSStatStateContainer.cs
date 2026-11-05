
using UnityEngine;

public class KSStatStateContainer : KSStateContainer
{
    public EquippedSlotPanel[] equippedSlotPanels;
    public KingSlimeBody characterBody;
    public CurrentKSAbilityInfoPanel[] currentKSAbilityInfoPanels; //현재 끼고 있는 장비 별 능력치 정보
    public override void OpenContainer()
    {

    }
    public override void UpdatePanel()
    {
        foreach (EquippedSlotPanel panel in equippedSlotPanels)
        {
            panel.UpdatePanel();
        }

        characterBody.UpdateCharacter();

        for (int i = 0; i < currentKSAbilityInfoPanels.Length; i++)
        {
            currentKSAbilityInfoPanels[i].UpdatePanel(UserDataManager.Instance.userData.characterName);
        }
    }
}