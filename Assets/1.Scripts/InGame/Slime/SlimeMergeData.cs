using System.Linq;
using UnityEngine;


[CreateAssetMenu]
public class SlimeMergeData : ScriptableObject
{
    public string key;
    public string[] growth1SlimeKeys; //1개면 있으면
    public bool sell;//판매하고 구매해야지 뽑을 수 있음

    public bool CanSpawn()
    {
        if (sell)
        {
            if (!UserManager.Instance.userSlimeManager.GetUserSlime(key).own)
            {
                return false;
            }
        }
        string[] equiptedSlimeKeys = UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes
            .Select(slime => slime.key)
            .ToArray();

        return growth1SlimeKeys.All(equiptedSlimeKeys.Contains);
    }
}