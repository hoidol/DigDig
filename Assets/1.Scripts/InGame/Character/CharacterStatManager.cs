
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterStatManager
{
    [SerializeField] List<CharacterStat> statList = new();
    public Dictionary<StatType, CharacterStat> statDic = new Dictionary<StatType, CharacterStat>();
    public Dictionary<string, CharacterSlimeData> slimeDic = new Dictionary<string, CharacterSlimeData>();

    // public Dictionary<LevelUpStatType, CharacterLevelUpStat> levelUpStatDic = new Dictionary<LevelUpStatType, CharacterLevelUpStat>();
    public Dictionary<string, CharacterItemStat> itemStatDic = new Dictionary<string, CharacterItemStat>();
    public List<Buff> activeBuffs = new List<Buff>();

    public float MaxHp => statDic[StatType.MaxHp].value;
    public float AttackPower => statDic[StatType.AttackPower].value;
    public float MoveSpeed => statDic[StatType.MoveSpeed].value;
    public float RecoveryHp => statDic[StatType.RecoveryHp].value;
    public float AttackSpeed => statDic[StatType.AttackSpeed].value;
    public float CritChance => statDic[StatType.CritChance].value;
    public float CritPower => statDic[StatType.CritPower].value;
    // public float AmmoEfficiency => statDic[StatType.AmmoEfficiency].value;
    // public int Bounce => (int)statDic[StatType.Bounce].value;

    public CharacterData characterData;
    public Character character;

    StatType[] usingStatTypes =
    {
        StatType.MaxHp,StatType.AttackPower,StatType.MoveSpeed,
        StatType.RecoveryHp,StatType.AttackSpeed,StatType.CritChance,
        StatType.CritPower,StatType.Dodge
        // StatType.AmmoEfficiency,
        // StatType.Bounce
    };

    public CharacterStatManager(Character p, CharacterName characterName)
    {
        character = p;

        characterData = CharacterManager.Instance.GetCharacterData(characterName);

        statDic.Clear();
        statList.Clear();

        for (int i = 0; i < usingStatTypes.Length; i++)
        {
            var ps = new CharacterStat { statType = usingStatTypes[i] };
            // Debug.Log($"ps.statType {ps.statType}");
            ps.initValue = characterData.GetCharacterStat(ps.statType).value; //CharacterData 초기화
            statList.Add(ps);
            statDic.Add(ps.statType, ps);
        }

        // for (int i = 0; i < LevelUpStatManager.Instance.levelUpStatDatas.Length; i++)
        // {
        //     var pl = new CharacterLevelUpStat { type = LevelUpStatManager.Instance.levelUpStatDatas[i].type };
        //     levelUpStatDic.Add(pl.type, pl);
        // }

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



        foreach (var buff in activeBuffs)
        {
            var stat = statDic[buff.statType];
            stat.value = buff.Apply(stat.value);
        }
    }

    // public void AddLevelUpState(LevelUpStatType type, int lv)
    // {
    //     if (!levelUpStatDic.ContainsKey(type))
    //     {
    //         levelUpStatDic.Add(type, new CharacterLevelUpStat()
    //         {
    //             type = type,
    //             lv = 0
    //         });
    //     }
    //     levelUpStatDic[type].lv += lv;
    // }
    public void AddItem(string key, int count=1)
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

    public void AddSlime(string key, int count = 1)
    {
        if (!slimeDic.ContainsKey(key))
        {
            slimeDic.Add(key, new CharacterSlimeData()
            {
                key = key,
                count = 0
            });
        }
        slimeDic[key].count += count;
        if (slimeDic[key].count <= 0)
        {
            slimeDic.Remove(key);
        }
    }
    public void RemoveSlime(string key)
    {
        if (slimeDic.ContainsKey(key))
        {
            slimeDic[key].count -= 1;
            if (slimeDic[key].count <= 0)
            {
                //아이템 제거하기
                slimeDic.Remove(key);
            }
        }
    }
    // public void AddOre(OreType oreType, int count = 1)
    // {
    //     if (!oreDic.ContainsKey(oreType))
    //     {
    //         oreDic.Add(oreType, new CharacterOre(){ oreType = oreType, count = count});
    //     }
    //     oreDic[oreType].count +=count;
    // }
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