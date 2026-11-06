using UnityEngine;


// [S] 사냥꾼: 3회 공격마다 저격 관통탄 발사 → 맞은 적 스턴 0.2초 + 치명 피해(확정 치명타)
[CreateAssetMenu(fileName = "Hunter", menuName = "SlimeData/S/Hunter")]
public class HunterSlimeData : EnhanceAbilitySlimeData
{
    public int snipeInterval = 3;        // N번째 공격마다 저격탄
    public int snipePierceCount = 99;    // 저격탄 관통 수 (사실상 무제한)
    public float snipeStunDuration = 0.2f;

    public override string GetDescription(int mergeLv = 0)
    {
        return $"{snipeInterval}회 공격마다 저격 관통탄을 발사해 {snipeStunDuration}초 스턴 + 치명 피해를 줍니다.";
    }
}
