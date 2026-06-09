using UnityEngine;

public class BlessingManager : MonoSingleton<BlessingManager> 
{
    public BlessingStone blessingPrefab;// 자동으로 획득됨 
    public void SpawnBlessingStone()
    {
        // BlessingStone blessingStone = Instantiate(blessingPrefab);
        // blessingStone.Spawn();
    }
}