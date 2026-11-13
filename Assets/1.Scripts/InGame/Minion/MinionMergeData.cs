using System.Linq;
using UnityEngine;


[CreateAssetMenu]
public class MinionMergeData : ScriptableObject
{
    public string key;
    public string[] growth1MinionKeys; //1개면 있으면
    public bool sell;//판매하고 구매해야지 뽑을 수 있음

    public bool CanSpawn()
    {
        if (sell)
        {
            if (!UserDataManager.Instance.userMinionManager.GetUserMinion(key).own)
            {
                return false;
            }
        }
        string[] equiptedMinionKeys = UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions
            .Select(minion => minion.key)
            .ToArray();

        return growth1MinionKeys.All(equiptedMinionKeys.Contains);
    }
}