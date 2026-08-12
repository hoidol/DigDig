using UnityEngine;
[System.Serializable]
public class AttackPowerLevelUpStatData : LevelUpStatData
{
    public float increaseValue = 1;
    public AttackPowerLevelUpStatData()
    {
        type = LevelUpStatType.AttackPower;
    }

    public override string GetDescription()
    {
        // int curLv = Character.Instance.statMgr.levelUpStatDic[type].lv;
        // int nextLv = curLv + 1;
        return ""; //return string.Format(TranslateManager.GetText($"{type}_description"), GetValue(curLv), GetValue(nextLv));
    }

    public float GetValue(int lv = -1)
    {
        // if (lv < 0)
        //     lv = Character.Instance.statMgr.levelUpStatDic[type].lv;
        return increaseValue * lv;
    }

}