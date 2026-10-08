using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveWarningPointerPoolingSystem : StackPoolingSystem<WaveWarningPointer>
{
    public override WaveWarningPointer Get(Vector3 pos, Transform parent = null)
    {
        if (prefab == null)
        {
            // Debug.Log("ExpPoolingSystem Get  if (prefab == null)");
            SetPrefab("Prefabs/WaveWarningPointer");
        }

        WaveWarningPointer waveWarningPointer = base.Get(pos, parent);
        return waveWarningPointer;
    }
}
