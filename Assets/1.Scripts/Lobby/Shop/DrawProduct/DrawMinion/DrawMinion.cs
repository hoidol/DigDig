
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DrawMinion", menuName = "ScriptableObjects/DrawMinion")]
public class DrawMinion : DrawSO
{
    // public string[] minionKeys;//중에 랜덤으로 하나?
        // [Header("등급에 따른 확률")]
        // public DrawMinionInfo[] drawMinionInfos;
    [Header("몇개 뽑을지")]
    public int count;

    public string[] Draw() => Draw(count);

    public string[] Draw(int count) => Draw(count, MinionManager.Instance.gradeGroupGrowth1MinionDic);
}

[System.Serializable]
public class DrawMinionInfo : DrawInfo
{
}
