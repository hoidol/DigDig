using TMPro;
using UnityEngine;

// 선택된 타일에 설치된 아이템 강화 (TileItemInfoPanel 안). 주사위 코인 = DiceCoin × (현재 레벨 + 1)을 먼저 내고
// 확률(TileItem.UPGRADE_CHANCES)로 성공하면 레벨 +1, 실패하면 코인만 소모. 강화 불가·최대 레벨이면 숨김
public class UpgradeTileItemButton : ButtonUI
{
    public TMP_Text upgradeInfoText;
    public TMP_Text priceText;
    Tile tile;

    public override void Awake()
    {
        base.Awake();
        if (priceText == null)
            priceText = GetComponentInChildren<TMP_Text>(true);
    }

    void OnEnable() => GameEventBus.Subscribe<DiceCoinEvent>(OnDiceCoin);
    void OnDisable() => GameEventBus.Unsubscribe<DiceCoinEvent>(OnDiceCoin);
    void OnDiceCoin(DiceCoinEvent e) => Refresh();

    public void SetTile(Tile tile)
    {
        this.tile = tile;
        Refresh();
    }

    TileItem Item => tile != null ? tile.tileItem as TileItem : null;

    void Refresh()
    {
        TileItem item = Item;
        bool show = item != null && item.CanUpgrade && !item.IsMaxLevel;
        gameObject.SetActive(show);
        if (!show)
            return;
        if (upgradeInfoText != null)
            upgradeInfoText.text = string.Format(TranslateManager.GetText("TileItem_Upgrade"), item.UpgradeChance);
    priceText.text = item.UpgradeCost.ToString();
        if (button != null)
            button.interactable = KingSlime.Instance.diceCoin >= item.UpgradeCost;
    }

    public override void OnClickedBtn()
    {
        TileItem item = Item;
        if (item == null || !item.CanUpgrade || item.IsMaxLevel)
            return;
        int cost = item.UpgradeCost;
        if (KingSlime.Instance.diceCoin < cost)
        {
            ToastCanvas.Toast(TranslateManager.GetText("Not enough dice coin"));
            return;
        }
        KingSlime.Instance.AddDiceCoin(-cost);
        bool success = item.TryUpgrade(Random.Range(0f, 100f));
        ToastCanvas.Toast(success
            ? string.Format(TranslateManager.GetText("TileItem_UpgradeSuccess"), item.upgradeLevel)
            : TranslateManager.GetText("TileItem_UpgradeFail"));

        // 이름(Lv)·버튼 갱신
        TileItemInfoPanel panel = GetComponentInParent<TileItemInfoPanel>(true);
        if (panel != null)
            panel.SetTile(tile);
        else
            Refresh();
    }
}
