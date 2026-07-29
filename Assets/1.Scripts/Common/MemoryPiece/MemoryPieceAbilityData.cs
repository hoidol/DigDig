using UnityEngine;


[CreateAssetMenu(fileName = "MemoryPieceAbilityData", menuName = "MemoryPieceAbilityData", order = 0)]
public class MemoryPieceAbilityData : ScriptableObject 
    
{
    public string key;   
    public int level;
}

//[업데이트 후]
// 공 / 방어 / 기타
// Step1 : 공격력 +0.5 / 체력 +3 / 공격속도 4% / 분산 투자 - 개당 30
// Step2 : 바운드 효율 +2% / 초당 회복 +0.1 / 경험치 +3%(MaxExp에 빼기로 구현) / 분산 투자 - 개당 30
// Step3 : 체력이 50% 미만일때 공격력 5%>10% 증가 / 회복 아이템 확률 +1% / [?] / 분산 투자 - 개당 30
// Step4 : [?] / 5% 확률로 공격으로 인한 체력 감소 무시 / [?] / 분산 투자 - 개당 30