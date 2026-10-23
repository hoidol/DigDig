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
        return (GameSetting.INIT_SPAWN_PRICE + GameManager.Instance.slimeSpawnCount * GameSetting.INCREASE_SPAWN_PRICE);
    }
    public override void OnClickedBtn()
    {
        // if (GameSetting.MAX_SLIME_SLOT_COUNT <= SlimeSpawner.Instance.activeslimes.Count)
        // {
        //     ToastCanvas.Toast(string.Format(TranslateManager.GetText("MaxSlime"), $"{GameSetting.MAX_SLIME_SLOT_COUNT}/{EnemySpawner.Instance.activeEnemies.Count}"));
        //     return;

        // }
        if (Character.Instance.coin < GetSpawnPrice())
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

        if(SlimeSpawner.Instance.slimeMaxCount <= Character.Instance.slimeInventory.curSlimes.Count)
        {
            ToastCanvas.Toast("Max Slime");
            return;
        }
        

        Character.Instance.AddCoin(-GetSpawnPrice());
        string randomSlimeKey = UserDataManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes[Random.Range(0, 5)].key;
        //Slime slime = Character.Instance.AddSlime("Base");
        Slime slime = Character.Instance.AddSlime(randomSlimeKey);        
        tile.SetTileObject(slime);

        CharacterManageCanvas.Instance.UpdateCanvas();
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