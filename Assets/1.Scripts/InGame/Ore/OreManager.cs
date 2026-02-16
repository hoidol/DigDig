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

}
