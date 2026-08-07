using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningIndicatorPoolingSystem : StackPoolingSystem<WarningIndicator>
{

    public override WarningIndicator Get(Vector3 pos, Transform parent = null)
    {
        if (prefab == null)
        {
            // Debug.Log("ExpPoolingSystem Get  if (prefab == null)");
            SetPrefab("Prefabs/WarningIndicator");
        }
        WarningIndicator warningIndicator = base.Get(pos, parent);
        warningIndicator.transform.position = pos;
        return warningIndicator;
    }
}
