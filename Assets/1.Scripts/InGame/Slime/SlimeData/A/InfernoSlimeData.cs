using UnityEngine;


// [A] 지옥: 자기 TileGroup의 enemyRoots 중 가장 가까운 2개 사이에 지옥불 → 안에 있는 적에게 공격 주기마다 피해
// TileGroup 3*3의 가운데 칸에 있으면 지옥불 없음. [각성] 지옥불 피해 시 화상 부여
[CreateAssetMenu(fileName = "Inferno", menuName = "SlimeData/A/Inferno")]
public class InfernoSlimeData : EnhanceAbilitySlimeData
{
    // 지옥불 폭 (경로와 수직 방향, 월드 단위). 길이는 두 EnemyRoot 사이 거리
    public float hellfireWidth = 1f;
    // [각성] 화상: 지속 시간(초), 초당 피해 = AttackPower × burnDpsRatio
    public float burnDuration = 2f;
    public float burnDpsRatio = 0.3f;

    public override string GetDescription(int mergeLv = 0)
    {
        return "가장 가까운 적 경로 구간에 지옥불을 피워 안에 있는 적에게 피해를 줍니다. (타일그룹 가운데 칸에서는 발동 안 함)";
    }
}
