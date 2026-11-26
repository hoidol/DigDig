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
// (2026-11-26) 탭 2개: 0 = 용사(보유 용사 목록), 1 = 수련(레벨 스탯, 이전 화면 이름 "용사 강화"). 씬에 번호로 저장됨 → 순서 바꾸지 말 것
public enum KSMenuType
{
    Hero,      // 0: 이전 LevelStat(강화) 버튼 자리
    LevelStat, // 1: 이전 Authority(유물) 버튼 자리
}