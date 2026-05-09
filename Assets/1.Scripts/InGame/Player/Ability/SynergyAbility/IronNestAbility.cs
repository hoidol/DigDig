using UnityEngine;

// 철새 둥지 - 광석 N개 파괴 시 버튼 활성화, 클릭하면 벙커 소환
public class IronNestAbility : SynergyAbility
{
    static readonly int thresholds = 15;

    public Bunker bunkerPrefab;

    int oreDestroyCount;
    bool bunkerActive;
    bool buttonShown;

    public override void OnEquip(Player player)
    {
        oreDestroyCount = 0;
        bunkerActive = false;
        buttonShown = false;
        GameEventBus.Subscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
        GameEventBus.Subscribe<IronNestSpawnRequestEvent>(OnSpawnRequested);
        // GameEventBus.Subscribe<UndergroundStartEvent>(OnUndergroundStartEvent);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
        GameEventBus.Unsubscribe<IronNestSpawnRequestEvent>(OnSpawnRequested);
    }


    void OnOreDestroyed(OreStoneDestroyedEvent e)
    {
        if (bunkerActive || buttonShown) return;

        oreDestroyCount++;
        if (oreDestroyCount >= thresholds)
        {
            oreDestroyCount = 0;
            buttonShown = true;
            GameEventBus.Publish(new IronNestReadyEvent(thresholds));
        }
    }

    void OnSpawnRequested(IronNestSpawnRequestEvent e)
    {
        buttonShown = false;
        SpawnBunker();
    }

    void SpawnBunker()
    {
        if (bunkerPrefab == null || bunkerActive) return;

        bunkerActive = true;
        Vector2 pos = Player.Instance.transform.position;
        var bunker = Instantiate(bunkerPrefab, pos, Quaternion.identity);
        bunker.Spawn(pos);
        bunker.onDestroyed += () => bunkerActive = false;
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"광석 {thresholds}개 파괴 시 벙커 소환 (체력 최대HP 40%)";
    }
}
