using UnityEngine;
[System.Serializable]
public class MaxHpLevelUpStatData  :LevelUpStatData
{
    public float increaseValue = 5;
    
    public MaxHpLevelUpStatData()
    {
        levelUpStatType= LevelUpStatType.MaxHp;
    }
    public override string GetDescription()
    {
        int baseHp = (int)Player.Instance.statMgr.playerData.GetPlayerStat(StatType.MaxHp).value;
        int curLv =Player.Instance.statMgr.levelUpStatDic[levelUpStatType].lv    ;
        int nextLv = curLv+1;
        return string.Format(TranslateManager.GetText($"{levelUpStatType}_description"),baseHp+GetValue(curLv),baseHp+GetValue(nextLv));
    }

    public float GetValue(int lv =-1)
    {
        if(lv <0)
            lv = Player.Instance.statMgr.levelUpStatDic[levelUpStatType].lv;

        return increaseValue *lv;    
    }
}