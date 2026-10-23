using Cysharp.Threading.Tasks;
using UnityEngine;

public class DrawItemButton : ButtonUI
{
    // TileItem 중 랜덤 1개를 뽑아 바로 지급 → ItemInventory가 객체를 만들어 랜덤 타일에 설치
    public override void OnClickedBtn()
    {
        Tile tile =  TileManager.Instance.GetRandomTileForItem();
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

        Character.Instance.AddItem(itemData.key).Forget();
    }
}
