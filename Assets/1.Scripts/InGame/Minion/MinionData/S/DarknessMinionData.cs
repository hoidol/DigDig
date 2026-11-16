using UnityEngine;


// [S] 다크니스: 스킬 쿨타임(DarknessMinion.SKILL_COOLTIME)이 차면 선택 → 머리 위 목록에서 장착 미니언 중 원하는 종으로 변신 (머지 레벨 유지, 실패 없음)
[CreateAssetMenu(fileName = "Darkness", menuName = "MinionData/S/Darkness")]
public class DarknessMinionData : EnhanceMinionData
{
    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Minion.Darkness.Desc"), DarknessMinion.SKILL_COOLTIME, DarknessMinion.TRANSFORM_SUCCESS_CHANCE * 100f, DarknessMinion.TRANSFORM_GROW_CHANCE * 100f);
    }
}
