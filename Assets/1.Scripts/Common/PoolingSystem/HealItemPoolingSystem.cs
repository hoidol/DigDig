using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealItemPoolingSystem : StackPoolingSystem<HealItem>
{

    public override HealItem Get(Vector3 pos, Transform parent = null)
    {
        if (prefab == null)
        {
            // Debug.Log("ExpPoolingSystem Get  if (prefab == null)");
            SetPrefab("Prefabs/HealItem");
        }

        HealItem healItem = base.Get(pos, parent);
        healItem.Droped(pos);
        return healItem;
    }
}
