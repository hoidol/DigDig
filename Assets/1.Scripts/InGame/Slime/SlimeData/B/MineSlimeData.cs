using UnityEngine;


// [B] 지뢰: 자기 TileGroup의 적 경로(enemyRoots)에 지뢰 설치
[CreateAssetMenu(fileName = "Mine", menuName = "SlimeData/B/Mine")]
public class MineSlimeData : EnhanceAbilitySlimeData
{
    // 머지 레벨별 동시에 깔아 둘 수 있는 지뢰 수 (머지할수록 증가)
    public int[] maxMineCounts = { 3, 4, 5, 6, 7 };
    // 머지 레벨별 폭발 범위(반지름)
    public float[] explodeRadiuses = { 1f, 1.1f, 1.2f, 1.3f, 1.4f };
    public int maxHitCount = 4;
    public float triggerRadius = 0.4f; // 적이 이 거리 안에 들어오면 폭발
    public float mineLifetime = 20f;   // 착지 후 유지 시간

    public override string GetDescription(int mergeLv = 0)
    {
        return $"적 이동 경로에 지뢰를 설치합니다. 밟으면 폭발해 주변 적에게 피해를 줍니다. (최대 {maxMineCounts.GetByMergeLevel(mergeLv)}개, 범위 {explodeRadiuses.GetByMergeLevel(mergeLv)}, 최대 {maxHitCount}기)";
    }
}
