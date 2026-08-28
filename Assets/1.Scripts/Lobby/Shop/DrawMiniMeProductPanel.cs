using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class DrawMiniMeProductPanel : ProductPanel 
{
    //어떤것들이 나올지 설정해야지
    public string[] miniMeKeys;//중에 랜덤으로 하나?
    public DrawMiniMe drawMiniMe;
    public override void Purchased()
    {
        base.Purchased();
        string[] keys = drawMiniMe.Draw();
        for(int i = 0; i < keys.Length; i++)
        {
            UserManager.Instance.userMiniMeManager.AddUserMiniMe(keys[i]);
        }
        Lobby.DrawMiniMeResultCanvas.Instance.OpenCanvas(keys);

    }
}

[System.Serializable]
public class DrawMiniMe
{
    [Header("등급에 따른 확률")]
    public DrawMiniMeInfo[] drawMiniMeInfos;
    [Header("몇개 뽑을지")]
    public int count;
    public string[] Draw()
    {
        float total = 0;
        float[] chances = new float[drawMiniMeInfos.Length + 1];
        for (int i = 0; i < drawMiniMeInfos.Length; i++)
        {
            total += drawMiniMeInfos[i].chance;
            chances[i + 1] = total;
        }
        
        var minimeManager = MiniMeManager.Instance;
        var picks = new (string key, Grade grade)[count];

        for (int i = 0; i < count; i++)
        {
            float pickChance = Random.Range(0, total);
            Grade grade = drawMiniMeInfos[drawMiniMeInfos.Length - 1].grade;
            for (int j = 0; j < chances.Length - 1; j++)
            {
                if (pickChance <= chances[j + 1])
                {
                    grade = drawMiniMeInfos[j].grade;
                    break;
                }
            }

            List<string> minimes = minimeManager.gradeGroupGrowth1MiniMeDic[grade];
            picks[i] = (minimes[Random.Range(0, minimes.Count)], grade);
        }

        return picks.OrderByDescending(p => p.grade).Select(p => p.key).ToArray();
    }
}
[System.Serializable]
public class DrawMiniMeInfo
{
    public Grade grade;
    public float chance;
}
