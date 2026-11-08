using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public  abstract class DrawSO: ScriptableObject
{
    public DrawInfo[] drawInfos;

    // drawInfos 확률로 등급을 고른 뒤 그 등급 목록에서 랜덤 count개 (높은 등급 순 정렬)
    // 해당 등급 아이템이 없으면(예: SS 데이터 없음) 그 등급은 빼고 남은 확률로 뽑는다
    protected string[] Draw(int count, Dictionary<GradeType, List<string>> gradeGroups)
    {
        DrawInfo[] infos = drawInfos.Where(i => i.chance > 0 && gradeGroups.TryGetValue(i.grade, out var keys) && keys.Count > 0).ToArray();
        if (infos.Length == 0)
        {
            Debug.LogError($"[{name}] 뽑을 수 있는 등급이 없음");
            return new string[0];
        }

        float total = infos.Sum(i => i.chance);
        var picks = new (string key, GradeType grade)[count];
        for (int i = 0; i < count; i++)
        {
            float pickChance = Random.Range(0, total);
            GradeType grade = infos[infos.Length - 1].grade;
            float acc = 0;
            foreach (DrawInfo info in infos)
            {
                acc += info.chance;
                if (pickChance <= acc)
                {
                    grade = info.grade;
                    break;
                }
            }

            List<string> keys = gradeGroups[grade];
            picks[i] = (keys[Random.Range(0, keys.Count)], grade);
        }

        return picks.OrderByDescending(p => p.grade).Select(p => p.key).ToArray();
    }
}
[System.Serializable]
public class DrawInfo
{    
    public GradeType grade;
    public float chance;
}
