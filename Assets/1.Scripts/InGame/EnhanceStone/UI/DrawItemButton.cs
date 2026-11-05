using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DrawItemButton : ButtonUI
{
    public TMP_Text priceText;
    public TMP_Text chanceText; // 100%% -> 95%

    void Start()
    {
        UpdateButton();
    }
    void UpdateButton()
    {
        if (priceText != null)
            priceText.text = GetDrawPrice().ToString();
        if (chanceText != null)
            chanceText.text = $"{GetDrawChance()}%";
    }
    // 처음 10코인, 뽑을 때마다 2코인씩 증가
    int GetDrawPrice()
    {
        return GameSetting.INIT_DRAW_ITEM_PRICE + GameManager.Instance.drawItemCount * GameSetting.INCREASE_DRAW_ITEM_PRICE;
    }

    // 성공 확률(%): 처음 100%, 성공할 때마다 5%씩 감소 (최소 MIN_DRAW_ITEM_CHANCE)
    public static int GetDrawChance()
    {
        int chance = GameSetting.INIT_DRAW_ITEM_CHANCE - GameManager.Instance.drawItemCount * GameSetting.DECREASE_DRAW_ITEM_CHANCE;
        return Mathf.Max(chance, GameSetting.MIN_DRAW_ITEM_CHANCE);
    }

    // 코인을 내고 SelectItemCanvas를 연다 (확률 판정 → 아이템 3개 중 선택 → 타일 선택)
    public override void OnClickedBtn()
    {
        if (KingSlime.Instance.coin < GetDrawPrice())
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
        if (ItemManager.Instance.GetDrawTileItems(1).Count == 0)
        {
            Debug.Log("DrawItemButton 뽑을 타일 아이템이 없거나 설치할 타일 없음");
            return;
        }

        KingSlime.Instance.AddCoin(-GetDrawPrice());
        SelectItemCanvas.Instance.OpenDraw(GetDrawChance(), UpdateButton);
    }
}
