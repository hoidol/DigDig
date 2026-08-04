using UnityEngine;
[System.Serializable]
public class BounceLevelUpStatData : LevelUpStatData
{
    int increaseValue = 1;
    public BounceLevelUpStatData()
    {
        type = LevelUpStatType.Bounce;
    }

    public override string GetDescription()
    {
        int curLv = Player.Instance.statMgr.levelUpStatDic[type].lv;
        int nextLv = curLv + 1;
        return ""; //return string.Format(TranslateManager.GetText($"{type}_description"), GetValue(curLv), GetValue(nextLv));
    }

    public int GetValue(int lv = -1)
    {
        if (lv < 0)
            lv = Player.Instance.statMgr.levelUpStatDic[type].lv;
        return increaseValue * lv;
    }
}