using UnityEngine;


// [S] 희생: 같은 머지 레벨의 아무 슬라임과 합성 가능 → 결과는 상대 슬라임과 같은 종, 머지 레벨 +1
[CreateAssetMenu(fileName = "Sacrifice", menuName = "SlimeData/S/Sacrifice")]
public class SacrificeSlimeData : EnhanceAbilitySlimeData
{
    public override string GetDescription(int mergeLv = 0)
    {
        return "같은 별의 아무 슬라임과 합치면 그 슬라임으로 강화됩니다.";
    }
}
