using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class UniqueDrawSlimeProductPanel : DrawSlimeProductPanel
{
    [Header("특별 뽑기 확률표 (A등급 이상만)")]
    public DrawSlime specialDrawSlime;

    public override void UpdatePanel()
    {
        base.UpdatePanel();
        if (specialDrawInfoText != null)
            specialDrawInfoText.text = $"{Grade.GetGradeText(GradeType.A)} 이상 슬라임 등장까지 [{SpecialDrawProgress}/{specialDrawInterval}]";
    }

    // 특별 뽑기 회차: 1개는 specialDrawSlime에서 확정, 나머지는 평소대로
    protected override string[] Draw(int count)
    {
        if (!IsSpecialDraw)
            return base.Draw(count);
        return specialDrawSlime.Draw(1).Concat(base.Draw(count - 1)).ToArray();
    }
}
