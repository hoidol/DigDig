using UnityEngine;


// [S] 조커: 선택 후 변신 버튼 → 장착 슬라임 중 원하는 종으로 변신 (머지 레벨 유지, 30% 확률로 실패: 별 -1, 별 1이면 사라짐)
[CreateAssetMenu(fileName = "Joker", menuName = "SlimeData/S/Joker")]
public class JokerSlimeData : EnhanceSlimeData
{
    public float levelDownChance = 0.3f; // 변신 시 머지 레벨 -1 확률 (패널티). 강화 5/8/11 = 15/10/5%

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Slime.Joker.Desc"), levelDownChance * 100f);
    }
}
