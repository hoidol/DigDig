using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatInventory : MonoBehaviour
{
    public List<Stat> ownStats = new List<Stat>();


    public int statTotalCount
    {
        get; set;
    }
    void Start()
    {
#if UNITY_EDITOR
        GameEventBus.Subscribe<StartGameEvent>(OnStartGame);
#endif
    }
#if UNITY_EDITOR
    void OnStartGame(StartGameEvent e)
    {
        ownStats.Clear();
    }
#endif




    public bool HasStat(StatType type)
    {
        return ownStats.Any(s => s.statType == type);
    }

    public Stat GetStat(StatType type)
    {
        return ownStats.FirstOrDefault(s => s.statType == type);
    }

    public void AddStat(string key)
    {
        AddStat(StatData.GetStatData(key));
    }

    public void AddStat(StatData statData)
    {
        Stat stat = ownStats.FirstOrDefault(e => e.statType == statData.statType);
        if (stat != null)
        {
            stat.LevelUp();
        }
        else
        {
            stat = new Stat(statData.statType);
            stat.LevelUp();
            ownStats.Add(stat);
        }

        statTotalCount++;
        GameEventBus.Publish(new AddedStatEvent(statData));
        Player.Instance.UpdatePlayer();
    }


}


public class AddedStatEvent
{
    public StatData statData;
    public AddedStatEvent(StatData aData)
    {
        this.statData = aData;
    }
}
