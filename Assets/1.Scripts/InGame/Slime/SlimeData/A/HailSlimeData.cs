using UnityEngine;


// [A] 빙석: 내 타일이 속한 TileGroup의 적 경로(enemyRoots)를 순서대로 빙빙 도는 눈(HailStone) 소환
// 닿은 적에게 피해 + 확률로 얼음(슬로우)
[CreateAssetMenu(fileName = "Hail", menuName = "SlimeData/A/Hail")]
public class HailSlimeData : EnhanceSlimeData
{
    public int hailCount = 1;          // 눈 개수. 여러 개면 경로 위에 같은 간격으로
    public RotationDirection rotationDirection = RotationDirection.Clockwise; // 경로를 도는 방향 (화면 기준)
    public float moveSpeed = 3f;      // 공속 50(초당 1회) 기준 이동 속도(월드 단위/초). 공속 버프가 이동 속도에 반영
    public float hitCooldown = 0.5f;   // 같은 적을 다시 때리기까지 간격
    // mergeLv별 얼음(슬로우) 확률 (%)
    public float[] iceChances = { 30f, 35f, 40f, 45f, 50f };
    public float iceDuration = 1f;

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Slime.Hail.Desc"), iceChances.GetByMergeLevel(mergeLv));
    }
}
