
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class KingSlimeStatManager
{
    [SerializeField] List<KingSlimeStat> statList = new();
    public Dictionary<StatType, KingSlimeStat> statDic = new Dictionary<StatType, KingSlimeStat>();
    public Dictionary<string, InGameUserSlimeData> slimeDic = new Dictionary<string, InGameUserSlimeData>();

    // public Dictionary<LevelUpStatType, CharacterLevelUpStat> levelUpStatDic = new Dictionary<LevelUpStatType, CharacterLevelUpStat>();
    public Dictionary<string, CharacterItemStat> itemStatDic = new Dictionary<string, CharacterItemStat>();
    public List<Buff> activeBuffs = new List<Buff>();

    public float MaxHp => statDic[StatType.MaxHp].value;
    public float AttackPower => statDic[StatType.AttackPower].value;
    public float AttackSpeed => statDic[StatType.AttackSpeed].value;
    public float CritChance => statDic[StatType.CritChance].value;
    public float CritPower => statDic[StatType.CritPower].value;
    public float AttackRange => statDic[StatType.AttackRange].value;

    public KingSlimeData kingslimeData;
    public KingSlime kingslime;

    // StatType 전부 (StatType이 추가되면 자동 포함)
    StatType[] usingStatTypes = Enumerable.Range(0, (int)StatType.Count).Select(i => (StatType)i).ToArray();

    public float GetValue(StatType statType) => statDic.TryGetValue(statType, out var stat) ? stat.value : 0f;

    public KingSlimeStatManager(KingSlime p, KingSlimeName kingslimeName)
    {
        kingslime = p;

        kingslimeData = KingSlimeManager.Instance.GetKingSlimeData(kingslimeName);

        statDic.Clear();
        statList.Clear();

        for (int i = 0; i < usingStatTypes.Length; i++)
        {
            var ps = new KingSlimeStat { statType = usingStatTypes[i] };
            // Debug.Log($"ps.statType {ps.statType}");
            ps.initValue = KingSlimeManager.Instance.GetTotalStatValue(kingslimeData, ps.statType); //CharacterData + 장비 + 로비 업그레이드
            statList.Add(ps);
            statDic.Add(ps.statType, ps);
        }

        // for (int i = 0; i < LevelUpStatManager.Instance.levelUpStatDatas.Length; i++)
        // {
        //     var pl = new CharacterLevelUpStat { type = LevelUpStatManager.Instance.levelUpStatDatas[i].type };
        //     levelUpStatDic.Add(pl.type, pl);
        // }

        slimeDic.Clear();
        UserDataManager.Instance.userSlimeManager.userSlimeData.userSlimes.ForEach(s =>
        {
            slimeDic.Add(s.key, new InGameUserSlimeData()
            {
                key = s.key,
                count = 0,
                reinforceLevel = 0
            });
        });

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
        // itemStatDic[key].sum += count;
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
            slimeDic.Add(key, new InGameUserSlimeData()
            {
                key = key,
                count = 0
            });
        }
        slimeDic[key].count += count;
    }

    public int GetReinforceLevel(string key) => slimeDic.TryGetValue(key, out var slime) ? slime.reinforceLevel : 0;

    // 필드에 그 종이 있는지 (강화는 필드에 있을 때만)
    public bool HasSlimeOnField(string key) => slimeDic.TryGetValue(key, out var slime) && slime.count > 0;

    public void LevelUpSlime(string key)
    {
        if (slimeDic.ContainsKey(key))
        {
            slimeDic[key].reinforceLevel += 1;
            GameEventBus.Publish(new SlimeLevelUpReinforceEvent() { key = key, reinforceLevel = slimeDic[key].reinforceLevel });
        }

    }

    public void RemoveSlime(string key)
    {
        // 수가 0이 돼도 항목은 남긴다: 머지로 그 종이 잠깐 사라져도 강화 레벨(종 단위 투자)은 유지
        if (slimeDic.ContainsKey(key))
            slimeDic[key].count = Mathf.Max(0, slimeDic[key].count - 1);
    }
    
    public CharacterItemStat GetCharacterItemStat(string key)
    {
        return itemStatDic[key];
    }
}

[System.Serializable]
public class KingSlimeStat
{
    public StatType statType;
    public float initValue; //CharacterData + EquipmentAbility
    public float value; //0~1

    public string Title => statType.ToString();
    public Sprite Thum
    {
        get
        {
            var sprite = Resources.Load<Sprite>($"Icon/Stat/{statType}");
            return sprite;
        }
    }

    // public string GetValueToString()
    // {
    //     return value.ToString();
    // }


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
    public int count; //현재 총 누적 개수
}

[System.Serializable]
public class SlimeLevelUpReinforceEvent
{
    public string key;
    public int reinforceLevel;
}