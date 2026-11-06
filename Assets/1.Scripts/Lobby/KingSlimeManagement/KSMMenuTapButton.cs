using UnityEngine;

public class KSMMenuTapButton : TapButton
{
    public KSMenuType menuType;
    
    public override void OnClickedBtn()
    {
        SoundManager.Instance.PlaySound(SFXType.Click);
        GetComponentInParent<KSManagementCanvas>().SelectMenu(menuType);
    }
}
public enum KSMenuType
{
    Upgrade,
    Equipment,
    Aura // 출시 후 업데이트 예정. 지금은 KSManagementCanvas.SelectMenu에서 무시 (값 순서 유지: 프리팹에 int로 저장됨)
}