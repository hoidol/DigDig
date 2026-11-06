using UnityEngine;


// [A] 폭격: 드래그로 옮기는 깃발 자리에 높은 공격력의 폭탄을 떨어뜨림 (공속 느림)
// 깃발 근처(폭발 범위 안)에 적이 있을 때만 발사. 폭발에 맞은 적은 50% 확률로 슬로우 (고정)
[CreateAssetMenu(fileName ="Mortar",menuName = "SlimeData/A/Mortar")]
public class MortarSlimeData : EnhanceAbilitySlimeData
{
    public float[] ranges = {1.5f,1.9f,2.3f,2.6f,2.9f,3.2f}; // mergeLv별 폭발 범위
    public int maxHitCount = 5;
    public float slowChance = 0.25f;  // 폭발에 맞은 적 슬로우 확률 (머지 레벨과 무관, 고정). [각성] 50%
    public float slowDuration = 1f;
    public float flightTime = 0.6f;   // 발사 → 착탄 시간
    public float arcHeight = 1.5f;    // 포물선 최고 높이

    public override string GetDescription(int level =0)
    {
        return $"깃발 위치에 폭탄을 떨어뜨립니다(최대 {maxHitCount}기). 맞은 적은 {slowChance * 100f:0}% 확률로 느려집니다.";
    }

}
