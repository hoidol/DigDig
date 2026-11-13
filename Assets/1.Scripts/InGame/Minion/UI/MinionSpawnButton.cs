using TMPro;
using UnityEngine;

public class MinionSpawnButton : ButtonUI
{
    public TMP_Text priceText;

    // 튜토리얼: 다음 소환에 나올 종·칸 지정 (장착 여부와 무관, 칸이 비어 있지 않으면 랜덤 칸). 한 번 소환하면 비움
    public static string forcedMinionKey;
    public static Tile forcedTile;

    void Start()
    {
        UpdateButton();
    }
    void UpdateButton()
    {
        priceText.text = GetSpawnPrice().ToString();
    }
    public static int GetSpawnPrice()
    {
        int price = GameSetting.INIT_SPAWN_PRICE + GameManager.Instance.minionSpawnCount * GameSetting.INCREASE_SPAWN_PRICE;
        if (AuthoritySystem.Has<MerchantLedgerAuthority>())
            price = MerchantLedgerAuthority.ApplyDiscount(price, GameManager.Instance.waveIdx);
        return price;
    }
    public override void OnClickedBtn()
    {
        // if (GameSetting.MAX_MINION_SLOT_COUNT <= MinionFieldController.Instance.activeminions.Count)
        // {
        //     ToastCanvas.Toast(string.Format(TranslateManager.GetText("MaxMinion"), $"{GameSetting.MAX_MINION_SLOT_COUNT}/{EnemyFieldController.Instance.activeEnemies.Count}"));
        //     return;

        // }
        if (KingSlime.Instance.coin < GetSpawnPrice())
        {
            ToastCanvas.Toast("Not enough coin");
            return;
        }

        Tile tile = forcedTile != null && forcedTile.IsEmpty ? forcedTile : TileManager.Instance.GetRandomEmptyTile();
        if (tile == null)
        {
            ToastCanvas.Toast("No empty tile");
            return;
        }

        if(MinionFieldController.Instance.minionMaxCount <= MinionFieldController.Instance.CountedMinionCount)
        {
            ToastCanvas.Toast("Max Minion");
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
        string randomMinionKey = !string.IsNullOrEmpty(forcedMinionKey)
            ? forcedMinionKey
            : UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions[Random.Range(0, 5)].key;
        forcedMinionKey = null;
        forcedTile = null;
        Minion minion = KingSlime.Instance.AddMinion(randomMinionKey, lv);
        tile.SetTileObject(minion);
        AchievementManager.Report(AchievementType.SpawnMinion); // 소환 버튼으로 뽑은 것만 (무료 소환 제외)

        KingSlimeManageCanvas.Instance.UpdateCanvas();
        UpdateButton();

    }
}
public class SpawnMinionEvent
{
    public string key;
    public int reinforce;
    public SpawnMinionEvent(string k, int r)
    {
        key = k;
        reinforce = r;
    }

}
public class RemoveMinionEvent
{
    public string key;
    public RemoveMinionEvent(string k)
    {
        key = k;
    }

}