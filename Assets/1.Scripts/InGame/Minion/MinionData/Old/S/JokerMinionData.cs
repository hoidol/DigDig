using UnityEngine;

// [S] 조커: JokerMinion.SKILL_COOLTIME초마다 카드(스페이드·다이아·클로버·하트) 효과 (수치는 JokerMinion const)
[CreateAssetMenu(fileName = "Joker", menuName = "MinionData/S/Joker")]
public class JokerMinionData : MinionData
{
    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Minion.Joker.Desc"), JokerMinion.SKILL_COOLTIME,
            JokerMinion.SPADE_DAMAGE_MUL * 100f, JokerMinion.DIAMOND_DURATION, JokerMinion.DIAMOND_ATTACK_SPEED_MUL);
    }
}
