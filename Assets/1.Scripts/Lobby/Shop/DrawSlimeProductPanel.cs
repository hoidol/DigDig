using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class DrawSlimeProductPanel : ProductPanel 
{
    //어떤것들이 나올지 설정해야지
    public string[] slimeKeys;//중에 랜덤으로 하나?
    public DrawSlime drawSlime;
    public override void Purchased()
    {
        base.Purchased();
        string[] keys = drawSlime.Draw();
        for(int i = 0; i < keys.Length; i++)
        {
            UserManager.Instance.userSlimeManager.AddUserSlime(keys[i]);
        }
        Lobby.DrawSlimeResultCanvas.Instance.OpenCanvas(keys);

    }
}

[System.Serializable]
public class DrawSlime
{
    [Header("등급에 따른 확률")]
    public DrawSlimeInfo[] drawSlimeInfos;
    [Header("몇개 뽑을지")]
    public int count;
    public string[] Draw()
    {
        float total = 0;
        float[] chances = new float[drawSlimeInfos.Length + 1];
        for (int i = 0; i < drawSlimeInfos.Length; i++)
        {
            total += drawSlimeInfos[i].chance;
            chances[i + 1] = total;
        }
        
        var minimeManager = SlimeManager.Instance;
        var picks = new (string key, GradeType grade)[count];

        for (int i = 0; i < count; i++)
        {
            float pickChance = Random.Range(0, total);
            GradeType grade = drawSlimeInfos[drawSlimeInfos.Length - 1].grade;
            for (int j = 0; j < chances.Length - 1; j++)
            {
                if (pickChance <= chances[j + 1])
                {
                    grade = drawSlimeInfos[j].grade;
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
public class DrawSlimeInfo
{
    public GradeType grade;
    public float chance;
}
