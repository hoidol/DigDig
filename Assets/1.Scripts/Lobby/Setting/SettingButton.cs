// 로비 설정 버튼 → 설정 창
public class SettingButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        SettingCanvas.Instance.OpenCanvas();
    }
}
