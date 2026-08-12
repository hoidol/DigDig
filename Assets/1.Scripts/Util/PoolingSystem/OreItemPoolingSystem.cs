using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrePiecePoolingSystem : StackPoolingSystem<OrePiece>
{

    public override OrePiece Get(Vector3 pos, Transform parent = null)
    {
        if (prefab == null)
        {
            // Debug.Log("ExpPoolingSystem Get  if (prefab == null)");
            SetPrefab("Prefabs/OreItem");
        }

        OrePiece orePiece = base.Get(pos, parent);
        orePiece.Droped(pos);
        return orePiece;
    }
}
