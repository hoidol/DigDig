using TMPro;
using UnityEngine;

public class SlimeSpawnButton : ButtonUI
{
    public TMP_Text priceText;

    void Start()
    {
        UpdateButton();
    }
    void UpdateButton()
    {
        priceText.text = GetSpawnPrice().ToString();
    }
    int GetSpawnPrice()
    {
        int price = GameSetting.INIT_SPAWN_PRICE + GameManager.Instance.slimeSpawnCount * GameSetting.INCREASE_SPAWN_PRICE;
        if (AuthoritySystem.Has<MerchantLedgerAuthority>())
            price = MerchantLedgerAuthority.ApplyDiscount(price, GameManager.Instance.waveIdx);
        return price;
    }
    public override void OnClickedBtn()
    {
        // if (GameSetting.MAX_SLIME_SLOT_COUNT <= SlimeFieldController.Instance.activeslimes.Count)
        // {
        //     ToastCanvas.Toast(string.Format(TranslateManager.GetText("MaxSlime"), $"{GameSetting.MAX_SLIME_SLOT_COUNT}/{EnemyFieldController.Instance.activeEnemies.Count}"));
        //     return;

        // }
        if (KingSlime.Instance.coin < GetSpawnPrice())
        {
            ToastCanvas.Toast("Not enough coin");
            return;
        }

        Tile tile = TileManager.Instance.GetRandomEmptyTile();
        if (tile == null)
        {
            ToastCanvas.Toast("No empty tile");
            return;
        }

        if(SlimeFieldController.Instance.slimeMaxCount <= SlimeFieldController.Instance.CountedSlimeCount)
        {
            ToastCanvas.Toast("Max Slime");
            return;
        }
        

        int cost = GetSpawnPrice();
        int lv = 0;
        if (AuthoritySystem.Has<GamblerCoinAuthority>())
        {
            (lv, cost) = GamblerCoinAuthority.Roll(Random.value, cost, KingSlime.Instance.coin);
            if (lv > 0)
                ToastCanvas.Toast(TranslateManager.GetText("Authority_GamblerCoinWin"));
            else if (cost > GetSpawnPrice())
                ToastCanvas.Toast(TranslateManager.GetText("Authority_GamblerCoinLose"));
        }
        KingSlime.Instance.AddCoin(-cost);
        string randomSlimeKey = UserDataManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes[Random.Range(0, 5)].key;
        Slime slime = KingSlime.Instance.AddSlime(randomSlimeKey, lv);
        tile.SetTileObject(slime);

        KingSlimeManageCanvas.Instance.UpdateCanvas();
        UpdateButton();

    }
}
public class SpawnSlimeEvent
{
    public string key;
    public int reinforce;
    public SpawnSlimeEvent(string k, int r)
    {
        key = k;
        reinforce = r;
    }

}
public class RemoveSlimeEvent
{
    public string key;
    public RemoveSlimeEvent(string k)
    {
        key = k;
    }

}