using System.Linq;
using UnityEngine;


[CreateAssetMenu]
public class MiniMeMergeData : ScriptableObject
{
    public string key;
    public string[] growth1MiniMeKeys; //1개면 있으면
    public bool sell;//판매하고 구매해야지 뽑을 수 있음

    public bool CanSpawn()
    {
        if (sell)
        {
            if (!UserManager.Instance.userMiniMeManager.GetUserMiniMe(key).own)
            {
                return false;
            }
        }
        string[] equiptedMiniMeKeys = UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes
            .Select(miniMe => miniMe.key)
            .ToArray();

        return growth1MiniMeKeys.All(equiptedMiniMeKeys.Contains);
    }
}