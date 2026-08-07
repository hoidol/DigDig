using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpPoolingSystem : StackPoolingSystem<Exp>
{

    public override Exp Get(Vector3 pos, Transform parent = null)
    {
        if (prefab == null)
        {
            // Debug.Log("ExpPoolingSystem Get  if (prefab == null)");
            SetPrefab("Prefabs/Exp");
        }

        Exp exp = base.Get(pos, parent);
        exp.Droped(pos);
        return exp;
    }
}
