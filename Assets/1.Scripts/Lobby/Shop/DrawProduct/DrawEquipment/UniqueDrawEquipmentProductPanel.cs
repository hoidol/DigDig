using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class UniqueDrawEquipmentProductPanel : DrawEquipmentProductPanel
{
    [Header("특별 뽑기 확률표 (A등급 이상만)")]
    public DrawEquipment specialDrawEquipment;

    public override void UpdatePanel()
    {
        base.UpdatePanel();
        if (specialDrawInfoText != null)
            specialDrawInfoText.text = string.Format(TranslateManager.GetText("Draw_SpecialEquipment"), Grade.GetGradeText(GradeType.A), SpecialDrawProgress, specialDrawInterval);
    }

    // 특별 뽑기 회차: 1개는 specialDrawEquipment에서 확정, 나머지는 평소대로
    protected override string[] Draw(int count)
    {
        if (!IsSpecialDraw)
            return base.Draw(count);
        return specialDrawEquipment.Draw(1).Concat(base.Draw(count - 1)).ToArray();
    }
}
