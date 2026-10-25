using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DrawItemButton : ButtonUI
{
    public TMP_Text priceText;

    void Start()
    {
        UpdateButton();
    }
    void UpdateButton()
    {
        if (priceText != null)
            priceText.text = GetDrawPrice().ToString();
    }
    // 처음 10코인, 뽑을 때마다 2코인씩 증가
    int GetDrawPrice()
    {
        return GameSetting.INIT_DRAW_ITEM_PRICE + GameManager.Instance.drawItemCount * GameSetting.INCREASE_DRAW_ITEM_PRICE;
    }

    // TileItem 중 랜덤 1개를 뽑아 바로 지급 → ItemInventory가 객체를 만들어 랜덤 타일에 설치
    public override void OnClickedBtn()
    {
        if (Character.Instance.coin < GetDrawPrice())
        {
            ToastCanvas.Toast("Not enough coin");
            return;
        }
        Tile tile = TileManager.Instance.GetRandomTileForItem();
        if (tile == null)
        {
            ToastCanvas.Toast("적용 가능한 타일이 없습니다.");
            return;
        }
        ItemData itemData = ItemManager.Instance.GetDrawTileItem();
        if (itemData == null)
        {
            Debug.Log("DrawItemButton 뽑을 타일 아이템이 없거나 설치할 타일 없음");
            return;
        }
        Debug.Log($"DrawITemButton OnClickedBtn() {itemData.key}");
        Character.Instance.AddCoin(-GetDrawPrice());
        GameManager.Instance.drawItemCount++;
        Character.Instance.AddItem(itemData.key).Forget();

        // CharacterManageCanvas.Instance.UpdateCanvas();
        UpdateButton();
    }
}
