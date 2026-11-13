using UnityEngine;


// [S] 희생: 같은 머지 레벨의 아무 슬라임과 합성 가능 → 결과는 상대 슬라임과 같은 종, 머지 레벨 +1
[CreateAssetMenu(fileName = "Sacrifice", menuName = "MinionData/S/Sacrifice")]
public class SacrificeMinionData : EnhanceMinionData
{
    public override string GetDescription(int mergeLv = 0)
    {
        return TranslateManager.GetText("Minion.Sacrifice.Desc");
    }
}
