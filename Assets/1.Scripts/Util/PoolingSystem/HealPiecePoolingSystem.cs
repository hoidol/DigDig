using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPiecePoolingSystem : StackPoolingSystem<HealPiece>
{

    public override HealPiece Get(Vector3 pos, Transform parent = null)
    {
        if (prefab == null)
        {
            // Debug.Log("ExpPoolingSystem Get  if (prefab == null)");
            SetPrefab("Prefabs/HealItem");
        }

        HealPiece healPiece = base.Get(pos, parent);
        healPiece.Droped(pos);
        return healPiece;
    }
}
