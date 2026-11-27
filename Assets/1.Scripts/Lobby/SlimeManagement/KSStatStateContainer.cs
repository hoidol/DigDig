using UnityEngine;

// KS 관리 화면 상단: 슬라임 능력치 (기본 + 레벨 스탯 포인트)
public class KSStatStateContainer : MonoBehaviour
{
    public CurrentKSAbilityInfoPanel[] currentKSAbilityInfoPanels;

    public void OpenContainer()
    {
    }

    // hero = 용사 탭에서 고른 용사 (null = 슬라임 기본). 용사면 그 용사 Lv.1 스탯 + 수련 포인트 (2026-11-26)
    public void UpdatePanel(HeroDataTable.HeroData hero = null)
    {
        foreach (CurrentKSAbilityInfoPanel panel in currentKSAbilityInfoPanels)
        {
            if (panel != null)
                panel.UpdatePanel(UserDataManager.Instance.userData.characterName, hero);
        }
    }
}
