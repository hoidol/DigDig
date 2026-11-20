using UnityEngine;

// 선택한 미니언 제거 (AllySettingPanel 안). (2026-11-20) 보상 없음 — 골드 개념 삭제로 판매 코인도 없앰, 자리만 비움
// 킹슬라임 선택 시엔 숨김. 제거 후 선택 해제
public class SellMinionButton : ButtonUI
{
    Minion minion;

    public void SetMinion(IAllyUnit allyUnit)
    {
        minion = allyUnit != null && allyUnit.AllyType != AllyType.Character
            ? allyUnit.Transform.GetComponent<Minion>()
            : null;

        gameObject.SetActive(minion != null);
    }

    // 제거 후 선택 해제 (RemoveFieldMinion이 선택 중인 타일이면 해제)
    public override void OnClickedBtn()
    {
        if (minion == null || !minion.gameObject.activeInHierarchy)
            return;

        Minion target = minion;
        minion = null;
        MinionFieldController.Instance.RemoveFieldMinion(target);
        SoundManager.Play(SFXType.ItemSell);
        MinionFieldController.Instance.DeselectTile();
    }
}
