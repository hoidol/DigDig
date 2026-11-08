
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DrawEquipment", menuName = "ScriptableObjects/DrawEquipment")]
public class DrawEquipment : DrawSO
{
       // public string[] equipmentKeys;//중에 랜덤으로 하나?

    // [Header("등급에 따른 확률")]
    // public DrawEquipmentInfo[] drawEquipmentInfos;
    [Header("몇개 뽑을지")]
    public int count;
    public string[] Draw() => Draw(count);

    public string[] Draw(int count) => Draw(count, EquipmentManager.Instance.gradeGroupEquipmentDic);
}
[System.Serializable]
public class DrawEquipmentInfo : DrawInfo
{
}
