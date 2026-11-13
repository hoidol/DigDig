using UnityEngine;


// [S] 시체수집: 공격 사거리 안에서 적이 처치되면(누가 잡든) 코인 획득
[CreateAssetMenu(fileName = "Scavenger", menuName = "MinionData/S/Scavenger")]
public class ScavengerMinionData : EnhanceMinionData
{
    public int coinPerKill = 1;

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Minion.Scavenger.Desc"), coinPerKill);
    }
}
