using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPoolingSystem : StackPoolingSystem<Coin>
{

    public override Coin Get(Vector3 pos, Transform parent = null)
    {
        if (prefab == null)
        {
            // Debug.Log("ExpPoolingSystem Get  if (prefab == null)");
            SetPrefab("Prefabs/OrePiece");
        }

        Coin coin = base.Get(pos, parent);
        coin.Droped(pos);
        return coin;
    }
}
