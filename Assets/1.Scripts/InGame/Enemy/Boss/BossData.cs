using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "EnemyData/BossData", order = 0)]
public class BossData : EnemyData
{
    [Tooltip("페이즈 전환 HP 비율 (예: 0.7, 0.4 → 70%, 40% 이하 시 다음 페이즈)")]
    public float[] phaseThresholds;
}
