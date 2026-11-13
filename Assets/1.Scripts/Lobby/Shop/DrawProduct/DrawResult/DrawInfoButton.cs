using UnityEngine;

// 뽑기 확률 정보 버튼: 상위 뽑기 패널(DrawMinionProductPanel = 일반 / UniqueDrawMinionProductPanel = 고급)의
// 확률표(drawMinion)를 DrawInfoCanvas로 보여준다
public class DrawInfoButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        DrawMinionProductPanel panel = GetComponentInParent<DrawMinionProductPanel>(true);
        if (panel == null || panel.drawMinion == null)
        {
            Debug.LogError($"[DrawInfoButton] 상위에 뽑기 패널(drawMinion)이 없음 ({name})");
            return;
        }
        if (DrawInfoCanvas.Instance != null)
            DrawInfoCanvas.Instance.OpenCanvas(panel.drawMinion);
    }
}
