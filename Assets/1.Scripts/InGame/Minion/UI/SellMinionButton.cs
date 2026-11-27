using UnityEngine;

// 선택한 미니언 제거 (AllySettingPanel 안). (2026-11-20) 골드 보상 없음
// (2026-11-23) 주사위 코인 환불(표시 머지 레벨만큼) → 다음 브레이크타임에 지급 (Slime.bonusDiceCoinNextBreak)
// 슬라임 선택 시엔 숨김. 제거 후 선택 해제
public class SellMinionButton : ButtonUI
{
    Minion minion;

    // (2026-11-23) 판매 환불 주사위 코인 = 표시 머지 레벨 (mergeLevel 0 → 1개, 1 → 2개, 2 → 3개 …)
    public static int GetRefundDiceCoin(int mergeLevel) => Mathf.Max(0, mergeLevel) + 1;

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
        // (2026-11-23) 주사위 코인 환불 = 표시 머지 레벨(mergeLevel + 1)개, 다음 브레이크타임에 지급 (보너스 엘리트와 같은 주머니)
        int refund = GetRefundDiceCoin(target.mergeLevel);
        // (2026-11-25) 브레이크타임 중이면 즉시 지급
        bool now = Slime.Instance.AddRewardDiceCoin(refund);
        DiceCoinText.Instantiate().SetDiceCoinText(target.transform.position, now ? $"+{refund}" : $"다음 웨이브 +{refund}");
        MinionFieldController.Instance.RemoveFieldMinion(target);
        SoundManager.Play(SFXType.ItemSell);
        MinionFieldController.Instance.DeselectTile();
    }
}
