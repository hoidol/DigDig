using UnityEngine;

public class KSMMenuTapButton : TapButton
{
    public KSMenuType menuType;
    
    public override void OnClickedBtn()
    {
        SoundManager.Play(SFXType.TabChange);
        GetComponentInParent<KSManagementCanvas>().SelectMenu(menuType);
    }
}
// (2026-11-24) 강화·권능 탭 → 레벨 스탯 하나로 통합. 씬에 남은 탭 버튼용으로 값만 유지 (번호로 저장됨)
public enum KSMenuType
{
    LevelStat, // 이전 Upgrade
    Unused,    // 이전 Authority
}