using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlessingStonePoolingSystem : StackPoolingSystem<BlessingStone>
{

   public override BlessingStone Get(Vector3 pos, Transform parent =null)
    {
        if(prefab == null)
            SetPrefab("BlessingStone");
        return base.Get(pos,parent);
    }
}
