using UnityEngine;


// [A] 지옥: 자기 TileGroup의 enemyRoots 중 가장 가까운 2개 사이에 지옥불 → 안에 있는 적에게 공격 주기마다 피해
// TileGroup 3*3의 가운데 칸에 있으면 지옥불 없음
[CreateAssetMenu(fileName = "Inferno", menuName = "MinionData/A/Inferno")]
public class InfernoMinionData : EnhanceMinionData
{
    // 지옥불 폭 (경로와 수직 방향, 월드 단위). 길이는 두 EnemyRoot 사이 거리
    public float hellfireWidth = 1f;
    // [각성] (진화) 화상: 지속 시간(초), 초당 피해 = AttackPower × burnDpsRatio
    public float burnDuration = 2f;
    public float burnDpsRatio = 0.3f;

    public override string GetDescription(int mergeLv = 0)
    {
        return TranslateManager.GetText("Minion.Inferno.Desc");
    }
}
