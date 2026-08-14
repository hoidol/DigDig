using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class DrawEquipmentProductPanel : ProductPanel 
{
    //어떤것들이 나올지 설정해야지
    public string[] equipmentKeys;//중에 랜덤으로 하나?
    public DrawEquipment drawEquipment;
    public override void Purchased()
    {
        base.Purchased();
        string[] keys = drawEquipment.Draw();
        for(int i = 0; i < keys.Length; i++)
        {
            UserManager.Instance.userEquipmentManager.AddUserEquipment(keys[i]);
        }
        
        DrawEquipmentResultCanvas.Instance.OpenCanvas(keys);

    }
}

[System.Serializable]
public class DrawEquipment
{
    [Header("등급에 따른 확률")]
    public DrawEquipmentInfo[] drawEquipmentInfos;
    [Header("몇개 뽑을지")]
    public int count;
    public string[] Draw()
    {
        float total = 0;
        float[] chances = new float[drawEquipmentInfos.Length + 1];
        for (int i = 0; i < drawEquipmentInfos.Length; i++)
        {
            total += drawEquipmentInfos[i].chance;
            chances[i + 1] = total;
        }
        
        var equipmentManager = EquipmentManager.Instance;
        var picks = new (string key, Grade grade)[count];

        for (int i = 0; i < count; i++)
        {
            float pickChance = Random.Range(0, total);
            Grade grade = drawEquipmentInfos[drawEquipmentInfos.Length - 1].grade;
            for (int j = 0; j < chances.Length - 1; j++)
            {
                if (pickChance <= chances[j + 1])
                {
                    grade = drawEquipmentInfos[j].grade;
                    break;
                }
            }

            List<string> equipments = equipmentManager.gradeGroupEquipmentDic[grade];
            picks[i] = (equipments[Random.Range(0, equipments.Count)], grade);
        }

        return picks.OrderByDescending(p => p.grade).Select(p => p.key).ToArray();
    }
}
[System.Serializable]
public class DrawEquipmentInfo
{
    public Grade grade;
    public float chance;
}
