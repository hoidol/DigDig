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
        if (GameSetting.MAX_SLIME_SLOT_COUNT <= EnemySpawner.Instance.activeEnemies.Count)
        {
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("MaxSlime"), $"{GameSetting.MAX_SLIME_SLOT_COUNT}/{EnemySpawner.Instance.activeEnemies.Count}"));
            return;

        }
        if (Character.Instance.coin < GetSpawnPrice())
        {
            ToastCanvas.Toast("Not enough coin");
            return;
        }

        Character.Instance.AddCoin(-GetSpawnPrice());
        Character.Instance.AddSlime("Base");

        CharacterManageCanvas.Instance.UpdateCanvas();
        UpdateButton();

    }
}
public class SpawnMinieEvent
{
    public string key;
    public int reinforce;
    public SpawnMinieEvent(string k, int r)
    {
        key = k;
        reinforce = r;
    }

}