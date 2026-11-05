
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

    public string[] Draw(int count)
    {
        float total = 0;
        float[] chances = new float[drawInfos.Length + 1];
        for (int i = 0; i < drawInfos.Length; i++)
        {
            total += drawInfos[i].chance;
            chances[i + 1] = total;
        }
        
        var minimeManager = SlimeManager.Instance;
        var picks = new (string key, GradeType grade)[count];

        for (int i = 0; i < count; i++)
        {
            float pickChance = Random.Range(0, total);
            GradeType grade = drawInfos[drawInfos.Length - 1].grade;
            for (int j = 0; j < chances.Length - 1; j++)
            {
                if (pickChance <= chances[j + 1])
                {
                    grade = drawInfos[j].grade;
                    break;
                }
            }

            List<string> minimes = minimeManager.gradeGroupGrowth1SlimeDic[grade];
            picks[i] = (minimes[Random.Range(0, minimes.Count)], grade);
        }

        return picks.OrderByDescending(p => p.grade).Select(p => p.key).ToArray();
    }
}

[System.Serializable]
public class DrawSlimeInfo : DrawInfo
{
}
