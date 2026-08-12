using UnityEngine;
[System.Serializable]
public class MaxHpLevelUpStatData : LevelUpStatData
{
    public float increaseValue = 5;

    public MaxHpLevelUpStatData()
    {
        type = LevelUpStatType.MaxHp;
    }
    public override string GetDescription()
    {
        // int baseHp = (int)Character.Instance.statMgr.characterData.GetCharacterStat(StatType.MaxHp).value;
        // int curLv = Character.Instance.statMgr.levelUpStatDic[type].lv;
        // int nextLv = curLv + 1;
        return ""; //return string.Format(TranslateManager.GetText($"{type}_description"), baseHp + GetValue(curLv), baseHp + GetValue(nextLv));
    }

    public float GetValue(int lv = -1)
    {
        // if (lv < 0)
        //     lv = Character.Instance.statMgr.levelUpStatDic[type].lv;

        return increaseValue * lv;
    }
}