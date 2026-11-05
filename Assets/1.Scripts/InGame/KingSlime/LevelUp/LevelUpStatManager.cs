using System.Linq;
using UnityEngine;

public class LevelUpStatManager : MonoSingleton<LevelUpStatManager>
{
    public LevelUpStatData[] levelUpStatDatas;
    void Awake()
    {
        levelUpStatDatas = new LevelUpStatData[(int)LevelUpStatType.Count];
        for (int i = 0; i < (int)LevelUpStatType.Count; i++)
        {
            LevelUpStatType levelUpStatType = (LevelUpStatType)i;
            System.Type dataType = System.Type.GetType($"{levelUpStatType}LevelUpStatData");
            if (dataType == null)
            {
                // Debug.LogWarning($"[LevelUpStatPanel] {levelUpStatType}LevelUpStatData 타입을 찾지 못함");
                // return;
                continue;
            }
            levelUpStatDatas[i] = (LevelUpStatData)System.Activator.CreateInstance(dataType);
            levelUpStatDatas[i].type = levelUpStatType;

            // Debug.LogWarning($"[LevelUpStatPanel] {levelUpStatType}LevelUpStatData 생성 완료 ");
        }
    }

    public LevelUpStatData GetLevelUpStatData(LevelUpStatType type)
    {
        // Debug.Log($"LevelUpStatManager GetLevelUpStatData {type}");
        return levelUpStatDatas.Where(e => e.type == type).FirstOrDefault();
    }

}
