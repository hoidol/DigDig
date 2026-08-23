using TMPro;
using UnityEngine;

public class MiniMeSpawnButton : ButtonUI
{
    public TMP_Text priceText;
    public int spawnCount;

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
        return (GameSetting.INIT_SPAWN_PRICE + spawnCount * GameSetting.INCREASE_SPAWN_PRICE);
    }
    public override void OnClickedBtn()
    {
        if (GameSetting.MAX_MiniMe_COUNT <= EnemySpawner.Instance.activeEnemies.Count)
        {
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("MaxMiniMe"), $"{GameSetting.MAX_MiniMe_COUNT}/{EnemySpawner.Instance.activeEnemies.Count}"));
            return;

        }


        if (Character.Instance.coin < GetSpawnPrice())
        {
            ToastCanvas.Toast("Not enough coin");
            return;
        }


        Character.Instance.AddCoin(-GetSpawnPrice());
        Character.Instance.AddMiniMe("Base");

        spawnCount++;
        UpdateButton();
    }
}
