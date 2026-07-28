using UnityEngine;
[System.Serializable]
public class AttackPowerLevelUpStatData :LevelUpStatData
{
    public float increaseValue = 1;
    public AttackPowerLevelUpStatData()
    {
        levelUpStatType= LevelUpStatType.AttackPower;
    }

    public override string GetDescription()
    {
        int curLv =Player.Instance.statMgr.levelUpStatDic[levelUpStatType].lv    ;
        int nextLv = curLv+1;
        return string.Format(TranslateManager.GetText($"{levelUpStatType}_description"),GetValue(curLv),GetValue(nextLv));
    }

    public float GetValue(int lv =-1)
    {
        if(lv <0)
            lv = Player.Instance.statMgr.levelUpStatDic[levelUpStatType].lv;
        return increaseValue *lv;    
    }

}