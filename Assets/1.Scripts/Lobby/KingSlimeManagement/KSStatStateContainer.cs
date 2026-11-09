using UnityEngine;

// KS 관리 화면 상단: 킹 능력치 (기본 + 업그레이드)
public class KSStatStateContainer : MonoBehaviour
{
    public CurrentKSAbilityInfoPanel[] currentKSAbilityInfoPanels;

    public void OpenContainer()
    {
    }

    public void UpdatePanel()
    {
        foreach (CurrentKSAbilityInfoPanel panel in currentKSAbilityInfoPanels)
        {
            if (panel != null)
                panel.UpdatePanel(UserDataManager.Instance.userData.characterName);
        }
    }
}
