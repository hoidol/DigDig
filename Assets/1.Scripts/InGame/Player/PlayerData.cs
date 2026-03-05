using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "PlayerData", order = 0)]
public class PlayerData : ScriptableObject
{
    public string key; // 

    public PlayerStat[] playerStats = new PlayerStat[(int)PlayerStatType.Count];
    // public float hp;
    // public float attackPower;
    // public float moveSpeed;
    // public float attackSpeed;
    // public float attackRange;
    // public float recoveryHp;

    public PlayerStat GetPlayerStat(PlayerStatType type)
    {
        for (int i = 0; i < playerStats.Length; i++)
        {
            if (playerStats[i].playerStatType == type)
            {
                return playerStats[i];
            }
        }
        return null;
    }
#if UNITY_EDITOR
    public void Edit()
    {
        for (int i = 0; i < playerStats.Length; i++)
        {
            playerStats[i].playerStatType = (PlayerStatType)i;
        }
    }
#endif
}