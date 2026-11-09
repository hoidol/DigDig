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
    Relic, // 이전 Equipment (프리팹에 int로 저장)
}