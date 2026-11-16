using UnityEngine;
using UnityEngine.UI;
using TMPro;
// (2026-11-14) 사용 안 함: 강화 칸은 ReinforceContainer로 화면에 바로 보임. 씬에서 정리한 뒤 삭제
public class ReinforceOpenButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        ReinforceCanvas.Instance.OpenCanvas();
    }
}
