using Unity.VisualScripting;
using UnityEngine;

public class OreManager : MonoSingleton<OreManager>
{

    public Ore orePrefab;
    public PoolingSystem poolingSystem;
    private void Awake()
    {
        poolingSystem = GetComponentInChildren<PoolingSystem>();
    }
    public Ore GetOre()
    {
        return poolingSystem.GetObject<Ore>();
    }

    void OnEnable()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    void OnDisable()
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);  // 반드시 해제
    }

    void OnEnemyDead(EnemyDeadEvent e)
    {
        Ore ore = GetOre();
        ore.Droped(e.position, "0");
    }

}
