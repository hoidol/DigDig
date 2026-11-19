using TMPro;
using UnityEngine;

// 선택한 미니언 판매 (AllySettingPanel 안). 가격 = SELL_MINION_BASE_PRICE + SELL_MINION_PRICE_PER_MERGE_LEVEL × mergeLevel (mergeLevel 0부터)
// 킹슬라임 선택 시엔 숨김
public class SellMinionButton : ButtonUI
{
    public TMP_Text priceText; // 비워도 됨
    Minion minion;

    public static int GetSellPrice(int mergeLevel)
        => GameSetting.SELL_MINION_BASE_PRICE + GameSetting.SELL_MINION_PRICE_PER_MERGE_LEVEL * mergeLevel;

    public void SetMinion(IAllyUnit allyUnit)
    {
        minion = allyUnit != null && allyUnit.AllyType != AllyType.Character
            ? allyUnit.Transform.GetComponent<Minion>()
            : null;

        gameObject.SetActive(minion != null);
        if (minion != null && priceText != null)
            priceText.text = $"+{GetSellPrice(minion.mergeLevel)}";
    }

    // 판매 후 선택 해제 (RemoveFieldMinion이 선택 중인 타일이면 해제)
    public override void OnClickedBtn()
    {
        if (minion == null || !minion.gameObject.activeInHierarchy)
            return;

        int price = GetSellPrice(minion.mergeLevel);
        Minion target = minion;
        minion = null;
        MinionFieldController.Instance.RemoveFieldMinion(target);
        KingSlime.Instance.AddCoin(price);
        SoundManager.Play(SFXType.ItemSell);
        MinionFieldController.Instance.DeselectTile();
    }
}
