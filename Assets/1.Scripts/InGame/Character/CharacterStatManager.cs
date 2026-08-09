
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterStatManager
{
    [SerializeField] List<CharacterStat> statList = new();
    public Dictionary<StatType, CharacterStat> statDic = new Dictionary<StatType, CharacterStat>();

    public Dictionary<LevelUpStatType, CharacterLevelUpStat> levelUpStatDic = new Dictionary<LevelUpStatType, CharacterLevelUpStat>();
    public Dictionary<string, CharacterItemStat> itemStatDic = new Dictionary<string, CharacterItemStat>();
    public List<Buff> activeBuffs = new List<Buff>();

    public float MaxHp => statDic[StatType.MaxHp].value;
    public float AttackPower => statDic[StatType.AttackPower].value;
    public float MoveSpeed => statDic[StatType.MoveSpeed].value;
    public float RecoveryHp => statDic[StatType.RecoveryHp].value;
    public float AttackSpeed => statDic[StatType.AttackSpeed].value;
    public float CritChance => statDic[StatType.CritChance].value;
    public float CritPower => statDic[StatType.CritPower].value;
    public float AmmoEfficiency => statDic[StatType.AmmoEfficiency].value;
    public int Bounce => (int)statDic[StatType.Bounce].value;

    public CharacterData characterData;
    public Character character;

    StatType[] usingStatTypes =
    {
        StatType.MaxHp,StatType.AttackPower,StatType.MoveSpeed,
        StatType.RecoveryHp,StatType.AttackSpeed,StatType.CritChance,
        StatType.CritPower,
        StatType.AmmoEfficiency,
        StatType.Bounce
    };

    public CharacterStatManager(Character p, CharacterName characterName)
    {
        character = p;
        Debug.Log($"CharacterStatManager characterName {characterName}");
        characterData = CharacterManager.Instance.GetCharacterData(characterName);
        if (characterData == null)
        {
            Debug.Log($"CharacterStatManager if(characterData == null)");
        }
        else
        {
            Debug.Log($"CharacterStatManager if(characterData != null)");
        }

        statDic.Clear();
        statList.Clear();

        for (int i = 0; i < usingStatTypes.Length; i++)
        {
            var ps = new CharacterStat { statType = usingStatTypes[i] };
            Debug.Log($"ps.statType {ps.statType}");
            ps.initValue = characterData.GetCharacterStat(ps.statType).value; //CharacterData 초기화
            statList.Add(ps);
            statDic.Add(ps.statType, ps);
        }

        for (int i = 0; i < LevelUpStatManager.Instance.levelUpStatDatas.Length; i++)
        {
            var pl = new CharacterLevelUpStat { type = LevelUpStatManager.Instance.levelUpStatDatas[i].type };
            levelUpStatDic.Add(pl.type, pl);
        }

        //장비 능력치 초기값에 적용
        UserEquipment[] equippedEquipments = UserManager.Instance.userEquipmentManager.GetEquippedUserEquipments();
        for (int i = 0; i < equippedEquipments.Length; i++)
        {
            foreach (var statType in usingStatTypes)
            {
                statDic[statType].initValue += equippedEquipments[i].equipmentData.GetEquipmentAbility(statType).GetValue<float>();
            }
        }

        Reset();
    }

    void Reset()
    {
        for (int i = 0; i < usingStatTypes.Length; i++)
        {
            StatType statType = usingStatTypes[i];
            statDic[statType].value = statDic[statType].initValue;
        }
    }
    public void UpdateStat()
    {
        Reset();

        #region levelUpStat
        MaxHpLevelUpStatData maxHpLevelUpStatData = LevelUpStatManager.Instance.GetLevelUpStatData(LevelUpStatType.MaxHp) as MaxHpLevelUpStatData;
        BounceLevelUpStatData bounceLevelUpStatData = LevelUpStatManager.Instance.GetLevelUpStatData(LevelUpStatType.Bounce) as BounceLevelUpStatData;
        AttackPowerLevelUpStatData attackPowerLevelUpStatData = LevelUpStatManager.Instance.GetLevelUpStatData(LevelUpStatType.AttackPower) as AttackPowerLevelUpStatData;

        statDic[StatType.MaxHp].value += maxHpLevelUpStatData.GetValue();
        statDic[StatType.Bounce].value += bounceLevelUpStatData.GetValue();
        statDic[StatType.AttackPower].value += attackPowerLevelUpStatData.GetValue();

        #endregion
        // for (int i = 0; i < character.statInventory.ownStats.Count; i++)
        // {
        //     Stat stat = character.statInventory.ownStats[i];
        //     if (stat.lv <= 0) continue;
        //     StatData statData = StatData.GetStatData(stat.statType.ToString());

        //     var characterStat = statDic[stat.statType];
        //     characterStat.value = statData.Apply(characterStat.value, stat.lv);
        // }



        foreach (var buff in activeBuffs)
        {
            var stat = statDic[buff.statType];
            stat.value = buff.Apply(stat.value);
        }
    }

    public void AddLevelUpState(LevelUpStatType type, int lv)
    {
        if (!levelUpStatDic.ContainsKey(type))
        {
            levelUpStatDic.Add(type, new CharacterLevelUpStat()
            {
                type = type,
                lv = 0
            });
        }
        levelUpStatDic[type].lv += lv;
    }
    public void AddItem(string key, int count)
    {
        if (!itemStatDic.ContainsKey(key))
        {
            itemStatDic.Add(key, new CharacterItemStat()
            {
                key = key,
                count = 0
            });
        }
        itemStatDic[key].count += count;
        itemStatDic[key].sum += count;
        if (itemStatDic[key].count <= 0)
        {
            //아이템 제거하기
            itemStatDic.Remove(key);
        }
    }

    public CharacterItemStat GetCharacterItemStat(string key)
    {
        return itemStatDic[key];
    }
}

[System.Serializable]
public class CharacterStat
{
    public StatType statType;
    public float initValue; //CharacterData + EquipmentAbility
    public float value;
}


[System.Serializable]
public class CharacterLevelUpStat
{
    public LevelUpStatType type;
    public int lv;
}
[System.Serializable]
public class CharacterItemStat
{
    public string key;
    public int count; //현재
    public int sum;//누적
}