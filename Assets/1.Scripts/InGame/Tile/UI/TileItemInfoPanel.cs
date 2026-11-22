using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 선택된 타일의 아이템 이름/설명. 아이템 없는 타일은 표시 안 함
// (2026-11-22) 룬 강화(UpgradeTileItemButton)·제거(SellTileItemButton) 버튼 삭제
// (2026-11-21) 킹 버프 칸(빈 칸·미니언이 있는 칸)이면 같은 자리에 버프 칸 정보: 종족·직업·패시브가 주는 버프 (KingTileBuffArea.GetBuffDescription)
//   룬이 있는 버프 칸은 룬 설명 아래에 버프 칸 정보를 덧붙임
public class TileItemInfoPanel : MonoBehaviour
{
    public GameObject obj;
    public TMP_Text lvText;
    public TMP_Text descText;
    public Image thumImage;

    void Start()
    {
        obj.SetActive(false);
    }
    public void SetTile(Tile tile)
    {
        bool isBuffTile = KingTileBuffArea.IsBuffTile(tile);
        if (tile != null && tile.tileItem is not TileItem && isBuffTile)
        {
            SetBuffTile();
            return;
        }
        // 아이템이 설치된 타일만 표시 (위에 유닛이 있어도 아이템 없으면 숨김)
        if (tile == null || tile.tileItem is not TileItem item)
        {
            obj.SetActive(false);
            return;
        }
        ItemData data = item.itemData;
        // (2026-11-22) 룬 강화·제거 삭제 → 레벨 대신 룬 이름
        lvText.text = data.Title;
        descText.text = item.GetDescription();
        if (isBuffTile)
            descText.text += $"\n<size=85%>{KingTileBuffArea.Current.GetBuffDescription()}</size>";
        thumImage.sprite = data.thum;
        thumImage.gameObject.SetActive(data.thum != null);
        obj.SetActive(true);
    }
    // 룬 없는 버프 칸: 제목 "버프 칸", 그림 = 종족 아이콘
    void SetBuffTile()
    {
        lvText.text = "버프 칸";
        descText.text = KingTileBuffArea.Current.GetBuffDescription();
        RaceKingAbility race = KingSlime.Instance.abilityInventory.Get<RaceKingAbility>();
        Sprite icon = race != null && race.data != null ? race.data.thum : null;
        thumImage.sprite = icon;
        thumImage.gameObject.SetActive(icon != null);
        obj.SetActive(true);
    }

    public void UpdatePanel()
    {
        
    }
}
