
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DrawSlime", menuName = "ScriptableObjects/DrawSlime")]
public class DrawSlime : DrawSO
{
    // public string[] slimeKeys;//중에 랜덤으로 하나?
        // [Header("등급에 따른 확률")]
        // public DrawSlimeInfo[] drawSlimeInfos;
    [Header("몇개 뽑을지")]
    public int count;

    public string[] Draw() => Draw(count);

    public string[] Draw(int count) => Draw(count, SlimeManager.Instance.gradeGroupGrowth1SlimeDic);
}

[System.Serializable]
public class DrawSlimeInfo : DrawInfo
{
}
