using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 선택된 타일의 아이템 이름/설명. 아이템 없는 타일은 표시 안 함
public class TileItemInfoPanel : MonoBehaviour
{
    public GameObject obj;
    public TMP_Text nameText;
    public TMP_Text descText;
    public Image thumImage;
    public SellTileItemButton sellTileItemButton;

    void Start()
    {
        obj.SetActive(false);
    }
    public void SetTile(Tile tile)
    {
        // 아이템이 설치된 타일만 표시 (위에 유닛이 있어도 아이템 없으면 숨김)
        if (tile == null || tile.tileItem is not TileItem item)
        {
            obj.SetActive(false);
            return;
        }
        if (sellTileItemButton != null)
            sellTileItemButton.SetTile(tile);

        ItemData data = item.itemData;
        nameText.text = data.Title;
        descText.text = item.GetDescription();
        thumImage.sprite = data.thum;
        thumImage.gameObject.SetActive(data.thum != null);
        obj.SetActive(true);
    }
    public void UpdatePanel()
    {
        
    }
}
